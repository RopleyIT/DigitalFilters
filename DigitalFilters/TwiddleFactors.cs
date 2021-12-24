using System.Numerics;

namespace DigitalFilters
{
    public class TwiddleFactors
    {
        /// <summary>
        /// The number of twiddle factors per 360 degrees
        /// </summary>

        public int Resolution { get; init; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="N">The number of twiddle factors to compute
        /// and include, equally spaced on the complex unit circle</param>
        /// <exception cref="ArgumentException">The argument must be a
        /// positive power of two</exception>

        public TwiddleFactors(int N)
        {
            // Validate argument

            if (!IsPositivePowerOfTwo(N))
                throw new ArgumentException
                    ("Twiddle factor count must be a positive power of two");
            Resolution = N;
            twiddleFactors = CalculateTwiddleFactors(N);
        }

        private readonly Complex[] twiddleFactors;

        /// <summary>
        /// Initialise the twiddle factor array. The structure
        /// of the array is such that twiddle factors W^k_N are
        /// to be found at offsets N to N + N - 1, for k in the
        /// range 0 to N - 1. W^N_N is also mapped onto W^0_N.
        /// </summary>
        /// <param name="N">The number of twiddle factors around
        /// the complex unit circle that this TwiddleFactors
        /// object will remember</param>

        private static Complex[] CalculateTwiddleFactors(int N)
        {
            int quarterPointCount = N >> 2;
            Complex[] twiddles = new Complex[N << 1];
            double[] cosValues = new double[1 + quarterPointCount];
            for (int i = quarterPointCount; i >= 0; i--)
                cosValues[i] = Math.Cos(i * 2 * Math.PI / N);
            int quadrant2 = quarterPointCount << 1;
            int quadrant3 = quarterPointCount + quadrant2;

            // Fill in the full N-point twiddle factor table,
            // in the last half of the twiddle factor array

            for (int i = 0; i < quarterPointCount; i++)
            {
                var c = cosValues[i];
                var s = cosValues[quarterPointCount - i];
                twiddles[N + i] = new Complex(c, -s);
                twiddles[N + i + quarterPointCount] = new Complex(-s, -c);
                twiddles[N + i + quadrant2] = new Complex(-c, s);
                twiddles[N + i + quadrant3] = new Complex(s, c);
            }

            // Now capture all the factorised twiddle ratios in the bottom
            // half of the array. The arrangement is such that Twiddle(k, N)
            // is located at index N+k into the array.

            for (N >>= 1; N > 0; N >>= 1)
            {
                for (int i = N; i < (N << 1); i++)
                    twiddles[i] = twiddles[i << 1];
            }
            return twiddles;
        }

        public Complex Twiddle(int k, int N)
        {
            // Validation of arguments

            if (N <= 0 || (N & (N - 1)) != 0)
                throw new ArgumentException
                    ("Twiddle factor denominator must be a positive power of two");
            if (N > twiddleFactors.Length)
                throw new ArgumentException
                    ("Twiddle factor requested outside of constructed range");
            if (k < 0)
                k = N + k;
            if (k > N)
                throw new ArgumentException("Twiddle factor numerator must be between -N & N");

            // Deal with the element value beyond the end of the array

            if (k == N)
                return Complex.One;
            else
                return twiddleFactors[N + k];
        }

        /// <summary>
        /// Check to see whether an integer has only one bit
        /// set in it, i.e. it represents a non-zero, positive
        /// valued power of two
        /// </summary>
        /// <param name="i">The integer to be validated</param>
        /// <returns>True if positive and has a Hamming weight
        /// of unity</returns>

        public static bool IsPositivePowerOfTwo(int i)
            => i > 0 && (i & (i - 1)) == 0;
    }
}
