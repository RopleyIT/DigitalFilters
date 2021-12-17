using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DigitalFilters
{
    /// <summary>
    /// Class to perform Fast Fourier Transforms or their
    /// inverse. The object once instantiated is reusable
    /// for either forward or inverse transforms, but once
    /// created the size of the sample sets is fixed.
    /// </summary>
    
    public class FastFourierTransform
    {
        public Complex[] TwiddleFactors { get; init; }
        private readonly int numBits;

        /// <summary>
        /// Create an object capable of performing
        /// forward or inverse digital Fourier
        /// transforms
        /// </summary>
        /// <param name="log2Points"> The log to base 2
        /// of the number of samples the transform will
        /// be applied to. In practice this can be any
        /// integer value from 2 to 16, giving sample
        /// set sizes of 4 samples up to 65536</param>
        /// <exception cref="ArgumentException">Thrown
        /// if the argument is not an integer in the
        /// range 2 to 16</exception>
        
        public FastFourierTransform(int log2Points)
        {
            if (log2Points < 2 || log2Points > 16)
                throw new ArgumentException
                    ("Log to base 2 of number of points must be between 2 and 16 inclusive");
            numBits = log2Points;

            // Calculate the twiddle factors used by the transform

            TwiddleFactors = CalculateTwiddleFactors();
        }

        private int BitReverse(int i)
        { 
            int result = 0;
            for(int targetBit = 1 << (numBits-1); targetBit > 0; targetBit >>= 1)
            {
                if ((i & 1) != 0)
                    result |= targetBit;
                i >>= 1;
            }
            return result;
        }

        private Complex[] CalculateTwiddleFactors()
        {
            int quarterPointCount = 1 << (numBits - 2);
            var twiddles = new Complex[quarterPointCount << 2];
            double AngleDelta = Math.PI /(quarterPointCount << 1);
            double[] cosValues = new double[1 + quarterPointCount];
            for (int i = quarterPointCount; i >= 0; i--)
                cosValues[i] = Math.Cos(i * AngleDelta);
            int quadrant2 = quarterPointCount << 1;
            int quadrant3 = quarterPointCount + quadrant2;
            for (int i = 0; i < quarterPointCount; i++)
            {
                var c = cosValues[i];
                var s = cosValues[quarterPointCount - i];
                twiddles[i] = new Complex(c, -s);
                twiddles[i + quarterPointCount] = new Complex(-s, -c);
                twiddles[i + quadrant2] = new Complex(-c, s);
                twiddles[i + quadrant3] = new Complex(s, c);
            }
            return twiddles;
        }

        /// <summary>
        /// Perform a forward digital Fourier transform on a set
        /// of real input samples, generating the complex frequency
        /// samples that represent amplitude and phase but using
        /// complex cartesian coordinates, as per the convention
        /// for a DFT. Uses the Cooley Tukey Fast Fourier Transform
        /// approach.
        /// </summary>
        /// <param name="input">The input sequence of samples (or
        /// frequency samples if performing the inverse
        /// transform). The length of the sequence must be a
        /// power of two.</param>
        /// <returns>The sequence representing the computed
        /// transform</returns>

        public Complex[] ForwardTransform(double[] input)
        {
            return Transform(input.Select(r => new Complex(r, 0)).ToArray(), false);
        }

        /// <summary>
        /// Perform an inverse Fourier transform from frequency to
        /// time domain
        /// </summary>
        /// <param name="input">The set of complex frequency/phase
        /// samples to transfirm back to the time domain. If this is
        /// the same length as expected in the constructor, the
        /// complex frequency samples are expected to be paired,
        /// i.e. sample[length-i] is the complex conjugate of
        /// sample[i]. This is necessary to produce all real
        /// time domain samples after the inverse transform
        /// has been applied. Alternatively, the array of
        /// frequency samples can be half length + 1, in which
        /// case the algorithm will reconstruct the second
        /// half of the sample set so that they do obey the
        /// property described above.</param>
        /// <returns>The time domain waveform</returns>
        /// <exception cref="ArgumentException">Thrown if
        /// the number of samples in the frequency domain
        /// does not match the expectations described above.
        /// </exception>
        
        public double[] InverseTransform(Complex[] input)
        {
            Complex[] freqSamples;
            if (input.Length == TwiddleFactors.Length)
                freqSamples = input;
            else if (input.Length == 1 + TwiddleFactors.Length / 2)
            {
                freqSamples = new Complex[TwiddleFactors.Length];
                Array.Copy(input, freqSamples, input.Length);
                for (int i = 1; i < TwiddleFactors.Length / 2; i++)
                    freqSamples[^i] = Complex.Conjugate(freqSamples[i]);
            }
            else
                throw new ArgumentException("Inverse transform sample count incorrect");
            return Transform(freqSamples, true).Select(s => s.Real).ToArray();
        }

        /// <summary>
        /// Perform the forward or inverse fast Fourier transform
        /// on a sequence of points
        /// </summary>
        /// <param name="input">The input sequence of samples (or
        /// frequency samples if performing the inverse
        /// transform). The length of the sequence must be a
        /// power of two.</param>
        /// <param name="inverse">True if the inverse transform is
        /// to be performed, false for the forward transform</param>
        /// <returns>The sequence representing the computed
        /// transform</returns>
        /// <exception cref="ArgumentException">Thrown if the number of
        /// samples passed to the transform method does not match the
        /// number of samples established by the constructor</exception>

        public Complex[] Transform(Complex[] input, bool inverse)
        {
            if (input.Length != TwiddleFactors.Length)
                throw new ArgumentException("Wrong number of input samples fed to FFT");

            // Create a set of bit shuffled input samples from the input time domain data

            Complex[] samples = new Complex[input.Length];

            // First two butterflies need no complex multiplication

            Complex j = inverse ? -Complex.ImaginaryOne : Complex.ImaginaryOne;
            for(int i = 0; i < samples.Length; i += 4)
            {
                var pAddq = input[BitReverse(i)];
                var lower = input[BitReverse(i + 1)];
                var pSubq = pAddq - lower;
                pAddq += lower;
                var rAdds = input[BitReverse(i + 2)];
                lower = input[BitReverse(i + 3)];
                var rSubs = rAdds - lower;
                rAdds += lower;
                samples[i] = pAddq + rAdds;
                samples[i + 1] = pSubq - j * rSubs;
                samples[i + 2] = pAddq - rAdds;
                samples[i + 3] = pSubq + j * rSubs;
            }

            // Remaining butterflies need to be computed

            for(int groupSize = 8; groupSize <= samples.Length; groupSize <<= 1)
            {
                int numGroups = samples.Length / groupSize;
                for(int group = 0; group < numGroups; group++)
                {
                    for(int i = 0; i < (groupSize>>1); i++)
                    {
                        int upper = group * groupSize + i;
                        int lower = upper + (groupSize >> 1);
                        Butterfly(ref samples[upper], ref samples[lower], 
                            GetTwiddle(i * numGroups, inverse));
                    }
                }
            }
            if (inverse)
                samples = samples.Select(s => s / samples.Length).ToArray();
            return samples;
        }

        private Complex GetTwiddle(int i, bool inverse)
        {
            if (i == 0)
                return 1;
            if (inverse)
                return TwiddleFactors[^i];
            else
                return TwiddleFactors[i];
        }

        private static void Butterfly(ref Complex upper, ref Complex lower, Complex w)
        {
            Complex wq = lower * w;
            lower = upper - wq;
            upper += wq;
        }
    }
}
