using DigitalFilters;
using System.Diagnostics;
namespace Interpolators
{
    /// <summary>
    /// Shift the pitch of a waveform by altering the sampling rate
    /// using interpolation to generate the new samples.
    /// </summary>
    
    public class PitchShifter
    {
        /// <summary>
        /// The input stream of samples to be shifted in pitch.
        /// </summary>
        
        public IEnumerable<double> InputSamples { get; init; }

        /// <summary>
        /// The ratio between input and output sample rates.
        /// A value of 1.5 means the output rate is 1.5 
        /// times the input rate, resulting in a pitch shift
        /// of a major fifth downwards if the outupt samples
        /// are then played back at the input sample rate.
        /// </summary>
        
        public double ShiftFactor { get; init; }

        /// <summary>
        /// The window function used to reduce ripples in the
        /// frequency response of the interpolated waveform.
        /// Leaving this as null will use a simple rectangular
        /// window function.
        /// </summary>

        public Func<double, int, double>? WindowFunction
            { get; set; } = null;

        /// <summary>
        /// The number of samples to each side of the centre
        /// sample of the windowed sinc function that is
        /// used to calculate the interpolated samples. Higher
        /// values produce a higher fidelity output at the cost
        /// of increased delay between input and output
        /// waveforms, and increased CPU usage.
        /// </summary>

        public int SamplesEachSide { get; init; }

        // The range of input samples affected by the windowed
        // sinc function when generating output samples.

        private QueueArray<double> sampleWindow;

        /// <summary>
        /// Constructor for the pitch shifter.
        /// </summary>
        /// <param name="inputSamples">The input stream of 
        /// samples to be interpolated for pitch shifting.</param>
        /// <param name="shiftFactor">The frequency ratio
        /// for the pitch shift</param>
        /// <param name="samplesEachSide"> Number of samples
        /// to each side of the centre of the sin(x)/x
        /// main lobe to be used in calculating the 
        /// interpolated samples.</param>

        public PitchShifter(IEnumerable<double> inputSamples, 
            double shiftFactor, int samplesEachSide)
        {
            InputSamples = inputSamples;
            ShiftFactor = shiftFactor;
            SamplesEachSide = samplesEachSide;
            sampleWindow = new QueueArray<double>
                (2 * SamplesEachSide + 1);
        }

        /// <summary>
        /// Generate the sequence of frequency-shifted
        /// output samples from the input sample sequence
        /// </summary>
        /// <returns>The output samples after pitch shifting
        /// </returns>
        
        public IEnumerable<double> ShiftedSamples()
        {
            int currInputIndex = 0;
            int currOutputIndex = 0;
            bool finished = false;
            List<double> inputSamples = InputSamples.ToList();
            inputSamples.AddRange(Enumerable.Repeat<double>(0.0, 2 * SamplesEachSide + 1));
            IEnumerator<double> iterator = inputSamples.GetEnumerator();
            while (!finished)
            {
                currOutputIndex++;

                // Shift the input samples so that the output sample
                // lies within plus or minus half the input sample
                // period of the midpoint of the sinc window.

                int newInputIndex = NearestInputSampleIndex(currOutputIndex);
                while(currInputIndex < newInputIndex)
                {
                    finished = !iterator.MoveNext();
                    sampleWindow.Insert(finished ? 0 : iterator.Current);
                    currInputIndex++;
                }

                // Calculate the exact sample time offset from the centre
                // input sample of the window.

                double sampleTime = currOutputIndex / ShiftFactor - currInputIndex;
                yield return GenerateOutputSample(sampleTime);
            }
        }

        /// <summary>
        /// Given a selected index number of a sample in
        /// the output sequence, how many input samples
        /// should have been shifted in the input sequence.
        /// </summary>
        /// <param name="outputSampleIndex">The output
        /// sample index for which we want a shift
        /// count for input samples.</param>
        /// <returns>The input sample index</returns>

        private int NearestInputSampleIndex(int outputSampleIndex)
            => (int)Math.Round(outputSampleIndex / ShiftFactor);

        /// <summary>
        /// Compute the sin(x)/ x function based on a
        /// smapling frequency of 1 Hz.
        /// </summary>
        /// <param name="t">The time interval into the waveform,
        /// where 1.0 matches one sample inerval</param>
        /// <returns>The sinc function value for the input
        /// time 't'</returns>

        private double Sinc(double t) 
            => t == 0.0 ? 1.0 : Math.Sin(Math.PI * t) / (Math.PI * t);

        /// <summary>
        /// Compute the sinc function values, with the
        /// window function applied to the samples.
        /// </summary>
        /// <param name="t">The offset in time from the centre
        /// of the sinc function. Zeros occur at each non-zero
        /// multiple of integers either side of zero.</param>
        /// <returns>The windowed sinc value</returns>
        
        private double WindowedSinc(double t)
        {
            if (WindowFunction == null)
                return Sinc(t);
            return Sinc(t) * WindowFunction
                (t + SamplesEachSide, 2 * SamplesEachSide);
        }

        /// <summary>
        /// Calculate the sinc function, windowed or otherwise,
        /// delayed by offset sample intervals.
        /// </summary>
        /// <param name="t">The time interval into the waveform,
        /// where 1.0 matches one sample inerval</param>
        /// <param name="offset">The number of sample intervals
        /// by which the waveform is delayed</param>
        /// <returns>The shifted sinc function value for
        /// the input time 't'</returns>
        
        private double OffsetSinc(double t, int offset) 
            => WindowedSinc(t - offset);

        /// <summary>
        /// Given a time offset within plus or minus half
        /// the input sample period, calculate the output
        /// sample at the specified time offset.
        /// </summary>
        /// <param name="tOffset">The offset of the desired
        /// output sample from the centre of the sinc
        /// function</param>
        /// <returns>The windowed output sample</returns>
        
        private double GenerateOutputSample(double tOffset)
        {
            double outputSample = 0.0;
            for (int i = -SamplesEachSide; i <= SamplesEachSide; i++)
            {
                // Get the input sample at the offset
                // from the centre of the sinc function.
                
                double inputSample = sampleWindow[i + SamplesEachSide];
                outputSample += OffsetSinc(tOffset, i) * inputSample;
            }
            return outputSample;
        }
    }
}
