using System.ComponentModel;

namespace WavFileManager
{
    /// <summary>
    /// Class for creating a WAV file from one or more
    /// streams of input samples
    /// </summary>
    
    public class WavWriter
    {
        // Buffer size is chosen so that when full
        // it will be completely filled by 1, 2, 3
        // or 4 byte samples in any given channel.

        private const int BufferSize = 65536;

        /// <summary>
        /// Rate at which shamples should be played back. 
        /// Usually 44100 (CD rate) or 48000 (DAT rate) Hz.
        /// </summary>
        
        public int SampleRate { get; init; }

        /// <summary>
        /// NUmber of channels of sound to be played back
        /// simultaneously. 1 for mono, 2 for stereo.
        /// </summary>
        
        public int Channels { get; init; }

        /// <summary>
        /// The number of bits in each audio sample.
        /// Common values are 8, 16 or 24 for PCM audio,
        /// or 32 for floating point audio.
        /// </summary>
        
        public int BitsPerSample { get; init; }

        /// <summary>
        /// The path to the file where the WAV data is stored
        /// </summary>
        
        private string FilePath { get; init; }

        /// <summary>
        /// Constructor for the WavWriter class.
        /// </summary>
        /// <param name="path">Path to the output file</param>
        /// <param name="sampleRate">Playback sample rate</param>
        /// <param name="channels">Mono stereo or more</param>
        /// <param name="bitsPerSample">Granularity of sound
        /// sample amplitudes</param>

        public WavWriter(string path, int sampleRate, int channels, int bitsPerSample)
        {
            SampleRate = sampleRate;
            Channels = channels;
            BitsPerSample = bitsPerSample;
            FilePath = path;

            // Create the data buffers for output samples

            OutputBuffer = new List<byte>
                (BufferSize * Channels * BitsPerSample / 8);

            // Truncate the output file if it already exists

            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        // The bytes holding the RIFF/WAVE header that
        // precedes the list of sound samples

        private byte[] header = new byte[44];

        /// <summary>
        /// Inject a string of 8 bit ASCII characters
        /// into the header array at a nominated offset
        /// </summary>
        /// <param name="offset">The offset into the
        /// header array</param>
        /// <param name="s">The character string to
        /// insert into the header</param>
        
        private void AsciiStringAt(int offset, string s)
        {
            for(int i = 0; i < s.Length; i++)
                header[offset + i] = (byte)s[i];
        }

        /// <summary>
        /// Insert a two byte integer into the header
        /// using little-endian byte ordering
        /// </summary>
        /// <param name="offset">The offset into the
        /// header byte array</param>
        /// <param name="value">The integer value
        /// whose LS two bytes will be inserted in
        /// litte-endian byte order</param>
        
        private void ShortAt(int offset, short value)
        {
            header[offset] = (byte)value;
            header[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        /// <summary>
        /// Insert a four byte integer into the header
        /// using little-endian byte ordering
        /// </summary>
        /// <param name="offset">The offset into the
        /// header byte array</param>
        /// <param name="value">The integer value
        /// whose LS four bytes will be inserted in
        /// litte-endian byte order</param>

        private void LongAt(int offset, int value)
        {
            header[offset] = (byte)value;
            header[offset + 1] = (byte)((value >> 8) & 0xFF);
            header[offset + 2] = (byte)((value >> 16) & 0xFF);
            header[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        /// <summary>
        /// Initialise those fields of the header that are known
        /// at the time of constructing this WavWriter instance.
        /// The data byte count and hence the file size fields
        /// will be filled in later when the last samples have
        /// been written to the file.
        /// </summary>
        
        private void InitHeader()
        {
            AsciiStringAt(0, "RIFF");
            AsciiStringAt(8, "WAVEfmt ");
            LongAt(4, 36 + (dataByteCount * Channels)); // File size - 8 bytes
            header[16] = (byte)0x10; // Header format section length
            header[20] = (byte)(BitsPerSample >= 32 ? 3 : 1); // Float = 3, PCM = 1
            header[22] = (byte)Channels;
            LongAt(24, SampleRate);
            LongAt(28, SampleRate * Channels * BitsPerSample / 8); // Byte rate
            ShortAt(32, (short)(Channels * BitsPerSample / 8)); // Bytes per sample set
            ShortAt(34, (short)BitsPerSample);
            AsciiStringAt(36, "data");
            LongAt(40, dataByteCount * Channels); // Data size
        }

        // The tally of the number of bytes in the output
        // sample frames. This will be used to fill in
        // the RIFF/WAVE header's data size field, and
        // the file size field.

        private int dataByteCount = 0;

        // In memory lists of samples of various sizes to
        // support the various different PCM sample formats
        // and sizes. One list per channel.

        private List<byte> OutputBuffer;

        /// <summary>
        /// Output the 8, 16, 24 and 32 bit linear PCM samples
        /// to the corresponding channel's output buffer. Note:
        /// it is unlikely that 32 bit linear PCM samples
        /// would be used, but 8, 16 and 24 are in common use.
        /// </summary>
        /// <param name="value">The value to be written, held
        /// in the least significant biits of the integer.</param>
        
        private void WriteChannel(int value)
        {
            // Both Aarch64 and x86/x64 architectures are little endian

            byte[] bytes = BitConverter.GetBytes(value);
            for(int i = 0; i < BitsPerSample / 8; i++)
                OutputBuffer.Add(bytes[i]);
        }

        /// <summary>
        /// Output the 32 bit IEEE floating point sample
        /// </summary>
        /// <param name="value">The value to be written</param>p
        
        private void WriteChannel(float value)
        {
            // Both Aarch64 and x86/x64 architectures are little endian

            if (BitsPerSample < 32)
                throw new ArgumentException
                    ("Cannot write float samples to a WAV file set up"
                        + " for 8. 16 or 24-bit samples");

            byte[] bytes = BitConverter.GetBytes(value);
            OutputBuffer.AddRange(bytes);
        }

        /// <summary>
        /// Write one block of BufferSize frames of samples
        /// to the output .WAV file.
        /// </summary>
        /// <param name="force">If set to true, causes the
        /// buffer to be writen even if the buffer is
        /// not full. Used for the last few frames of a
        /// sound file.</param>

        private void FlushIfBufferFull(bool force)
        {
            if (OutputBuffer.Count >= BufferSize * Channels * BitsPerSample / 8 
                || force && OutputBuffer.Count > 0)
            {
                dataByteCount += OutputBuffer.Count;
                using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    // Write the latest version of the header

                    InitHeader(); // Updates file and data size fields
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Write(header, 0, header.Length);
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(OutputBuffer.ToArray(), 0, OutputBuffer.Count);
                    OutputBuffer.Clear();
                }
            }
        }

        /// <summary>
        /// Output a single linear PCM sample to a mono .WAV file
        /// </summary>
        /// <param name="value">The 6, 16, or 24 bit sample. Note
        /// that if the file is expecting float samples, these
        /// will be generated from the integer argument</param>

        public void WriteMono(int value)
        {
            WriteChannel(BitsPerSample >= 32 ? (float)value : value);
            FlushIfBufferFull(false);
        }

        /// <summary>
        /// Output a floating point sample to a mono .WAV file
        /// that has been set up to expect floating point samples.
        /// If this is not the case, an exception will be thrown.
        /// </summary>
        /// <param name="value">The sample value to be written</param>
        
        public void WriteMono(float value)
        {
            WriteChannel(value);
            FlushIfBufferFull(false);
        }

        /// <summary>
        /// Accommodate 64 bit samples by downsizing to 32 bit
        /// </summary>
        /// <param name="value">The 64 bit sample</param>
        
        public void WriteMono(double value)
            => WriteMono((float)value);

        /// <summary>
        /// Output a pair of simultaneous stereo samples to a
        /// stereo .WAV file. The samples are 8, 16, or 24 bits
        /// in size.
        /// </summary>
        /// <param name="left">The 6, 16, or 24 bit left channel 
        /// sample. Note that if the file is expecting float 
        /// samples, these will be generated from the integer 
        /// argument</param>
        /// <param name="right">The 6, 16, or 24 bit right channel 
        /// sample. Note that if the file is expecting float 
        /// samples, these will be generated from the integer 
        /// argument</param>

        public void WriteStereo(int left, int right)
        {
            WriteChannel(BitsPerSample >= 32 ? (float)left : left);
            if(Channels > 1)
                WriteChannel(BitsPerSample >= 32 ? (float)right : right);
            FlushIfBufferFull(false);
        }

        /// <summary>
        /// Output a pair of stereo samples to the stereo
        /// .WAV file. The samples are eexpected to be 32 bit
        /// floating point. If this is not the case, 
        /// an exception will be thrown.
        /// </summary>
        /// <param name="left">The sample for the left
        /// stereo channel</param>
        /// <param name="right">The sample for the right
        /// stereo channel</param>

        public void WriteStereo(float left, float right)
        {
            WriteChannel(left);
            if (Channels > 1)
                WriteChannel(right);
        }

        /// <summary>
        /// Output a pair of stereo samples to the stereo
        /// .WAV file. The samples are eexpected to be 64 bit
        /// floating point. Note that they will be down-converted
        /// to 32 bit floating point samples.
        /// </summary>
        /// <param name="left">The sample for the left
        /// stereo channel</param>
        /// <param name="right">The sample for the right
        /// stereo channel</param>

        public void WriteStereo(double left, double right)
            => WriteStereo((float)left, (float)right);

        /// <summary>
        /// Force write the output bufer, 
        /// usually used at end of file.
        /// </summary>
        
        public void Flush()
            => FlushIfBufferFull(true);
    }
}
