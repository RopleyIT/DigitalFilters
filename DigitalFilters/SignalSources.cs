using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DigitalFilters
{
    /// <summary>
    /// Generate digital sample waveforms of various shapes
    /// </summary>
    
    public static class SignalSources
    {
        /// <summary>
        /// Create a single rectangular pulse followed by a train of zero values
        /// </summary>
        /// <param name="width">The width in samples of the pulse</param>
        /// <param name="duration">The overall duration in samples of the whole 
        /// waveform</param>
        /// <param name="magnitude">The height of the pulse, defaulting to 1
        /// </param>
        /// <returns></returns>
        
        public static IEnumerable<double> Impulse
            (int width, int duration, double magnitude = 1.0)
        {
            for (int i = 0; i < width; i++)
                yield return magnitude;
            for (int i = width; i < duration; i++)
                yield return 0.0;
        }

        /// <summary>
        /// Create a stream of samples that follow the shape of a sine wave
        /// </summary>
        /// <param name="frequency">The frequency of the sine wave in Hz</param>
        /// <param name="sampleRate">The number of samples per second</param>
        /// <param name="duration">The total number of samples in the
        /// sequence</param>
        /// <param name="magnitude">The amplitude of the sine wave, being
        /// half the peak to peak range of values</param>
        /// <returns>The sequence of sinewave samples</returns>
        
        public static IEnumerable<double> SineWave
            (int frequency, int sampleRate, int duration, double magnitude = 1.0)
        {
            for (int i = 0; i < duration; i++)
                yield return magnitude * Math.Sin(2 * i * Math.PI * frequency / sampleRate);
        }

        /// <summary>
        /// Create a single raised cosine wave shape, followed
        /// by a sequence of zero values
        /// </summary>
        /// <param name="width">The width in samples of the raised
        /// cosine pulse</param>
        /// <param name="duration">The overall length of the sequence</param>
        /// <param name="magnitude">The height of the raised cosine pulse
        /// at its centre peak value</param>
        /// <returns>The sequence of output samples</returns>
        
        public static IEnumerable<double> RaisedCosine
            (int width, int duration, double magnitude = 1.0)
        {
            for (int i = 0; i < width; i++)
                yield return magnitude * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (double)width));
            for (int i = width; i < duration; i++)
                yield return 0;
        }

        /// <summary>
        /// Create a sequence of gaussianly distributed white noise
        /// </summary>
        /// <param name="sampleCount">The number of samples in the sequence</param>
        /// <param name="gain">A factor to adjust the amplitude of the noise</param>
        /// <returns>The Gaussian noise sequence</returns>
        
        public static IEnumerable<double> WhiteNoise(int sampleCount, double gain = 1.0)
        {
            for (int i = 0; i < sampleCount; i++)
                yield return Gaussian() * gain;
        }

        /// <summary>
        /// Create a spectrally flat noise signal by using a constant
        /// magnitude but random phase for all frequency samples in a
        /// spectrum, then take the inverse FFT to convert it into a
        /// noise sequence. Brilliant for plotting the spectrum of a
        /// filter's output, by feeding this sequence into a filter,
        /// then taking the forward FFT of the filter's output sequence.
        /// </summary>
        /// <param name="sampleCount">The number of samples in the
        /// noise sequence. This must be a power of two, as it will
        /// emerge from an Inverse FFT</param>
        /// <param name="gain">The magnitude for each frequency in
        /// the spectrum</param>
        /// <returns>The spectrally flat noise signal</returns>
        /// <exception cref="ArgumentException">Thrown if the
        /// numer of samples requested is not a power of two
        /// </exception>
        
        public static IEnumerable<double> SyntheticNoise(int sampleCount, double gain = 1.0)
        {
            if (!TwiddleFactors.IsPositivePowerOfTwo(sampleCount))
                throw new ArgumentException("Length of sequence must be power of two");
            Random r = new Random((int)(DateTime.Now.Ticks));
            Complex[] freqSamples = new Complex[sampleCount/2 + 1];
            FastFourierTransform fft = new FastFourierTransform(sampleCount);
            for (int i = 0; i < freqSamples.Length; i++)
            {
                double angle = r.NextDouble() * 2 * Math.PI;
                freqSamples[i] = new Complex(gain * Math.Cos(angle), gain * Math.Sin(angle));
            }
            double[] results = fft.InverseTransform(freqSamples);
            foreach(double result in results)
                yield return result;
        }

        static readonly Random random = new((int)(DateTime.Now.Ticks));

        /// <summary>
        /// Simplistic Gaussian Noise generator
        /// </summary>
        /// <returns>A sample that has a Gausssian probability for
        /// its amplitude</returns>
        
        public static double Gaussian()
        {
            double v = 0;
            for (int i = 0; i < 8192; i++)
                v += 2 * random.NextDouble() - 1;
            return v;
        }

    }
}
