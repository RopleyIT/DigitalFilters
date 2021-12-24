namespace DigitalFilters
{
    public interface IFilter
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

        bool HighPass { get; }

        /// <summary>
        /// Order of the filter, minimum value 1
        /// </summary>

        public int Order { get; }

        /// <summary>
        /// Cut off frequency of filter in
        /// radians per second. Divide by 2π
        /// to obtain frequency in Hertz.
        /// </summary>

        public double CutOff { get; }

        /// <summary>
        /// The set of second order and first order polynomials
        /// representing the filter. These 2nd and 1st order
        /// filters are cascaded to obtain the overall 
        /// Butterworth filter.
        /// </summary>

        public IReadOnlyList<ComplexPoly> Polynomials { get; }
    }
}
