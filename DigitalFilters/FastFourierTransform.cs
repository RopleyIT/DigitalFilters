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
        public TwiddleFactors Twiddles { get; init; }
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

        public FastFourierTransform(int numSamples)
        {
            if (!TwiddleFactors.IsPositivePowerOfTwo(numSamples)
                || numSamples < 4 || numSamples > 65536)
                throw new ArgumentException
                    ("Number of points in transform a power of 2, from 4 to 65536");

            // Calculate the twiddle factors used by the transform.
            // For a 32 point transform, we find 64 points, so that
            // when doing a forward 64 real sample to 32 complex
            // frequency sample transform, we have the 64 twiddle
            // factors that we need.

            Twiddles = new(numSamples << 1);

            // Calculate the number of bits in the indices

            for (numBits = 0; numSamples > 1; numBits++)
                numSamples >>= 1;
        }

        private int BitReverse(int i)
        {
            int result = 0;
            for (int targetBit = 1 << (numBits - 1); targetBit > 0; targetBit >>= 1)
            {
                if ((i & 1) != 0)
                    result |= targetBit;
                i >>= 1;
            }
            return result;
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
            // Map the real input samples onto a half-length set of
            // complex samples, with even samples in the real part
            // and odd samples in the imaginary part.

            Complex[] samples = new Complex[input.Length >> 1];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = new Complex(input[i << 1], input[1 + (i << 1)]);
            samples = Transform(samples, false);

            // Length of output array includes the middle sample at the
            // Nyquist rate. This is necessary in case we need to perform
            // an inverse transform later.

            Complex[] results = new Complex[samples.Length + 1];
            for (int i = 0; i < samples.Length; i++)
            {
                var j = i == 0 ? 0 : (samples.Length - i);
                double xpr = (samples[i].Real + samples[j].Real) / 2;
                double xmr = (samples[i].Real - samples[j].Real) / 2;
                double xpi = (samples[i].Imaginary + samples[j].Imaginary) / 2;
                double xmi = (samples[i].Imaginary - samples[j].Imaginary) / 2;
                Complex twiddle = Twiddles.Twiddle(i, samples.Length << 1);
                double real = xpr + twiddle.Real * xpi + twiddle.Imaginary * xmr;
                double imag = xmi + twiddle.Imaginary * xpi - twiddle.Real * xmr;
                if (i == 0)
                    results[samples.Length] = new Complex(xpr - xpi, xmi + xmr);
                results[i] = new Complex(real, imag);
            }
            return results;
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
            if (!TwiddleFactors.IsPositivePowerOfTwo(input.Length - 1))
                throw new ArgumentException("Frequency samples must be 2^N + 1 in length");
            if (input.Length >= Twiddles.Resolution)
                throw new ArgumentException("Twiddle factors too coarse for this number of samples");
            freqSamples = new Complex[(input.Length - 1) << 1];
            Array.Copy(input, freqSamples, input.Length);
            for (int i = 1; i < input.Length - 1; i++)
                freqSamples[^i] = Complex.Conjugate(freqSamples[i]);
            return Transform(freqSamples, true).Select(s => s.Real).ToArray();
        }

        /// <summary>
        /// Perform the forward or inverse fast Fourier transform
        /// on a sequence of complex points
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
            if (!TwiddleFactors.IsPositivePowerOfTwo(input.Length))
                throw new ArgumentException("Frequency samples must be 2^N in length");
            if (input.Length > Twiddles.Resolution)
                throw new ArgumentException
                    ("Twiddle factors too coarse for this number of samples");

            // Create a set of bit shuffled input samples from the input data

            Complex[] samples = new Complex[input.Length];

            // First two butterflies need no complex multiplication

            Complex j = inverse ? -Complex.ImaginaryOne : Complex.ImaginaryOne;
            for (int i = 0; i < samples.Length; i += 4)
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

            for (int groupSize = 8; groupSize <= samples.Length; groupSize <<= 1)
            {
                int numGroups = samples.Length / groupSize;
                for (int group = 0; group < numGroups; group++)
                {
                    for (int i = 0; i < (groupSize >> 1); i++)
                    {
                        int upper = group * groupSize + i;
                        int lower = upper + (groupSize >> 1);

                        // The 2*i below is because we have twice as many
                        // twiddle factors as we need, in case we are
                        // implementing a transform of 2N real samples

                        int k = inverse ? -i : i;
                        Butterfly(ref samples[upper], ref samples[lower],
                            Twiddles.Twiddle(k * numGroups, samples.Length));
                    }
                }
            }
            if (inverse)
                samples = samples.Select(s => s / samples.Length).ToArray();
            return samples;
        }

        private static void Butterfly(ref Complex upper, ref Complex lower, Complex w)
        {
            Complex wq = lower * w;
            lower = upper - wq;
            upper += wq;
        }
    }
}
