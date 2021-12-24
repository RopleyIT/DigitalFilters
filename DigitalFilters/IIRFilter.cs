namespace DigitalFilters
{
    /// <summary>
    /// Infinite Impulse Response digital filter constructed
    /// using the Bilinear Z transform from an input analogue
    /// filter description, such as Butterworth, Chebychev,
    /// Elliptical etc.
    /// </summary>
    public class IIRFilter
    {
        /// <summary>
        /// Gain factor applied to filter output samples
        /// </summary>

        public double Gain { get; init; }

        /// <summary>
        /// The sampling rate for the digital filter, in Hz
        /// </summary>

        public double SamplingRate { get; init; }

        /// <summary>
        /// The sets of coeficients for each stage of the
        /// digital filter
        /// </summary>

        public List<IIRFilterStage> FilterStages { get; init; }

        /// <summary>
        /// The analog filter whence this digital filter was
        /// fashioned
        /// </summary>

        public IFilter AnalogueFilter { get; init; }

        /// <summary>
        /// Create an instance of an IIR filter from an
        /// equivalent analogue filter 's' domain specification
        /// </summary>
        /// <param name="analogueFilter">The equivalent analogue
        /// filter</param>
        /// <param name="samplingRate">The digital sampling rate
        /// for the digital filter</param>

        public IIRFilter(IFilter analogueFilter, double samplingRate, double gain = 1)
        {
            SamplingRate = samplingRate;
            AnalogueFilter = analogueFilter;
            Gain = gain;
            FilterStages = new(AnalogueFilter.Polynomials.Count);
            InitFilterStages();
        }

        // Find the tap coefficients for each of the analog filter's
        // first and second order polynomials

        private void InitFilterStages()
        {
            // Calculate bilinear Z prewarping factor

            double C = Math.Tan(AnalogueFilter.CutOff / (2 * SamplingRate));
            foreach (var poly in AnalogueFilter.Polynomials)
            {
                if (poly.Order == 2)
                    FilterStages.Add(InitSecondOrder(poly, C));
                else if (poly.Order == 1)
                    FilterStages.Add(InitFirstOrder(poly, C));
                else
                    throw new ArgumentException
                        ("Only first and second order polynomials permitted");
            }
        }

        // Compute the tap coefficients for a first order filter stage

        private IIRFilterStage InitFirstOrder(ComplexPoly poly, double C)
        {
            IIRFilterStage filterStage = new(poly.Order);
            double b0 = AnalogueFilter.HighPass ? 0 : 1;    // T(s) numerator offset coeff
            double b1 = AnalogueFilter.HighPass ? 1 : 0;    // T(s) numerator coeff of s
            double a1 = poly.Coefficients[1].Real;          // T(s) denom coeff of s
            double a0 = poly.Coefficients[0].Real;          // T(s) denom offsett coeff

            // Calculate denominator for each term in filter coefficients

            double denom = a1 + a0 * C;

            // Now calculate each of the coefficients themselves

            filterStage.CoeffX[0] = (b0 * C + b1) / denom;      // Coeff of x[n]
            filterStage.CoeffX[1] = (b0 * C - b1) / denom;      // Coeff of x[n-1]
            filterStage.CoeffY[0] = -(a0 * C - a1) / denom;     // Coeff of y[n-1]
            return filterStage;
        }

        // Compute the tap coeficients for one of the second order
        // filter stages

        private IIRFilterStage InitSecondOrder(ComplexPoly poly, double C)
        {
            IIRFilterStage filterStage = new(poly.Order);
            double b0 = AnalogueFilter.HighPass ? 0 : 1;    // T(s) numerator offset coeff
            double b2 = AnalogueFilter.HighPass ? 1 : 0;    // T(s) numerator coeff of s*2
            double a2 = poly.Coefficients[2].Real;          // T(s) denom coeff of s*s
            double a1 = poly.Coefficients[1].Real;          // T(s) denom coeff of s
            double a0 = poly.Coefficients[0].Real;          // T(s) denom offsett coeff

            // Calculate denominator for each term in filter coefficients

            double denom = a2 + (a1 + a0 * C) * C;

            // Now calculate each of the coefficients themselves

            filterStage.CoeffX[0] = (b2 + b0 * C * C) / denom;              // x[n]
            filterStage.CoeffX[1] = 2 * (b0 * C * C - b2) / denom;          // x[n-1]
            filterStage.CoeffX[2] = filterStage.CoeffX[0];                  // x[n-2]
            filterStage.CoeffY[0] = -2 * (a0 * C * C - a2) / denom;         // y[n-1]
            filterStage.CoeffY[1] = -(a2 + (a0 * C - a1) * C) / denom;      // y[n-2]
            return filterStage;
        }

        /// <summary>
        /// Attach a filter stage to a stream of input samples
        /// </summary>
        /// <param name="source">The input samples to be filtered</param>
        /// <returns>An enumerable for the output filtered samples</returns>

        private static IEnumerable<double> AddFilterStage
            (IIRFilterStage stage, IEnumerable<double> source)
        {
            double xPrev = 0, x2Prev = 0, yPrev = 0, y2Prev = 0;

            foreach (double d in source)
            {
                // Calculate the next output sample

                double output = d * stage.CoeffX[0]
                    + xPrev * stage.CoeffX[1] + yPrev * stage.CoeffY[0];
                if (stage.CoeffY.Length > 1)
                {
                    output += x2Prev * stage.CoeffX[2]
                        + y2Prev * stage.CoeffY[1];

                    // Now shift all the historic taps

                    x2Prev = xPrev;
                    y2Prev = yPrev;
                }
                xPrev = d;
                yPrev = output;
                yield return output;
            }
        }

        /// <summary>
        /// Attach an instance of this filter to a stream of input samples
        /// </summary>
        /// <param name="source">The unfiltered input stream of samples</param>
        /// <returns>The enumerable output stream from the filter</returns>

        public IEnumerable<double> Filter(IEnumerable<double> source)
        {
            IEnumerable<double> sink = source;
            foreach (var stage in FilterStages)
                sink = AddFilterStage(stage, sink);
            return sink.Select(s => s * Gain);
        }
    }
}
