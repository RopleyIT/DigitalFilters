using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DigitalFilters
{
    public class IIRFilter
    {
        public double[] CoeffX { get; init; }
        public double[] CoeffY { get; init; }

        private ComplexPoly DenomPoly;
        private double CutOff;
        private bool HighPass;
        private double SamplingRate;

        public IIRFilter(ComplexPoly poly, double cutOff, double samplingRate, bool highPass)
        {
            DenomPoly = poly;
            CutOff = cutOff;
            SamplingRate = samplingRate;
            HighPass = highPass;
            CoeffX = new double[3];
            CoeffY = new double[2];
            InitCoefficients();
        }

        private void InitCoefficients()
        {
            // First calculate the prewarping required for the cutoff frequency

            double C = Math.Tan(CutOff /(2 * SamplingRate));
            double b0 = HighPass ? 0 : 1;   // T(s) numerator offset coeff
            double b2 = HighPass ? 1 : 0;   // T(s) numerator coeff of s*2
            double a2 = DenomPoly.Coefficients[2].Real; // T(s) denom coeff of s*s
            double a1 = DenomPoly.Coefficients[1].Real; // T(s) denom coeff of s
            double a0 = DenomPoly.Coefficients[0].Real; // T(s) denom offsett coeff

            // Calculate denominator of gain factor for each coefficient

            double denom = a2 + (a1 + a0 * C) * C;

            // Now calculate each of the coefficients themselves

            CoeffX[0] = (b2 + b0 * C * C) / denom;              // x[n]
            CoeffX[1] = 2 * (b0 * C * C - b2) / denom;          // x[n-1]
            CoeffX[2] = CoeffX[0];                              // x[n-2]
            CoeffY[0] = -2 * (a0 * C * C - a2) / denom;         // y[n-1]
            CoeffY[1] = -(a2 + (a0 * C - a1) * C) / denom;      // y[n-2]
        }
    }
}
