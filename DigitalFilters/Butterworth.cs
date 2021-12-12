using System.Numerics;
namespace DigitalFilters
{
    public class Butterworth
    {
        /// <summary>
        /// True for a high pass filter, false
        /// for a low pass filter. For a high
        /// pass filter, we add Order zeros
        /// at the origin on the Laplace
        /// pole zero diagram. This is
        /// equivalent to setting the
        /// numerator to (s/CutOff)**Order
        /// if the ** operator existed as the
        /// power operator in C#!
        /// </summary>
        
        public bool HighPass { get; init; }

        /// <summary>
        /// Order of the filter, minimum value 1
        /// </summary>
        
        public int Order { get; init; }

        /// <summary>
        /// Cut off frequency of filter in
        /// radians per second. Divide by 2π
        /// to obtain frequency in Hertz.
        /// </summary>
        
        public double CutOff { get; init; }

        /// <summary>
        /// Construct a Butterworth filter
        /// </summary>
        /// <param name="order">The order of the
        /// filter</param>
        /// <param name="cutOff">The filter's 
        /// angular cutoff frequency</param>
        /// <param name="hiPass">Set true
        /// if this is to be a high pass filter
        /// </param>
        
        public Butterworth(int order, double cutOff, bool hiPass)
        {
            Order = order;
            CutOff = cutOff;
            HighPass = hiPass;
            InitPolynomials();
        }

        /// <summary>
        /// Find the location in the 's' plane of each LHHP pole
        /// for the Butterworth filter. Note that these are
        /// normalised to Cutoff = 1 rad/s. For a 1st order filter,
        /// this is at -1+0j giving a transfer function of
        /// 1/(s+1). For a 2nd order filter, there are two poles
        /// that are complex conjugates of each other at
        /// -.7071 + .7071j and -.7071 - .7071j giving a transfer
        /// function of 1/((s+0.7071-.7071j)(s+0.7071+.7071j))
        /// or when multiplied through, 1/(s*s + 1.414*s + 1).
        /// For denormalised filters, the Laplace variable 's'
        /// is replaced with s/CutOff, causing the two transfer
        /// functions above to become CutOff/(s+CutOff) and
        /// CutOff*CutOff/(s*s + 1.414*CutOff + CutOff*CutOff).
        /// Visually this has the effect of moving the poles out
        /// from the unit circle, onto a circle whose radius
        /// is equal to the angular cutoff frequency.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Complex Pole(int index)
        {
            if (index < 0 || index > Order)
                throw new ArgumentException("Pole indices 1 ... Order");
            double angle = Math.PI*(2*index + Order - 1)/(2*Order);
            return new Complex(Math.Cos(angle), Math.Sin(angle));
        }

        /// <summary>
        /// The set of second order and first order polynomials
        /// representing the filter. These 2nd and 1st order
        /// filters are cascaded to obtain the overall 
        /// Butterworth filter.
        /// </summary>
        
        public ComplexPoly[] Polynomials { get; private set; }

        private void InitPolynomials()
        {
            var polyCount = (Order + 1) / 2;
            Polynomials = new ComplexPoly[polyCount];
            if((Order & 1) != 0)
            {
                ComplexPoly poly = new();
                poly.Coefficients.Add(1);
                poly.Coefficients.Add(1 / CutOff);
                Polynomials[polyCount - 1] = poly;
            }
            for (int i = 1; i <= Order/2; i++)
            {
                var pole = Pole(i);
                ComplexPoly poly = new();
                poly.Coefficients.Add(pole.Real * pole.Real + pole.Imaginary * pole.Imaginary);
                poly.Coefficients.Add(-2 * pole.Real / CutOff);
                poly.Coefficients.Add(1 / (CutOff * CutOff));
                Polynomials[i-1] = poly;
            }
        }
    }
}