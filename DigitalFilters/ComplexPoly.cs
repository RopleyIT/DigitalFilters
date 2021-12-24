using System.Numerics;
namespace DigitalFilters
{
    public class ComplexPoly
    {
        public static readonly ComplexPoly ZeroPoly = new();

        public List<Complex> Coefficients { get; init; } = new();

        public int Order => Coefficients.Count - 1;

        private void ZeroCoefficients(int order)
        {
            Coefficients.Clear();
            for (int i = 0; i <= order; i++)
                Coefficients.Add(Complex.Zero);
        }

        public ComplexPoly MultiplyBy(ComplexPoly z)
        {
            // Deal with the edge case of a zero polynomial

            if (Order < 0 || z.Order < 0)
                return ZeroPoly;

            // First create a complete set of zero coefficients

            ComplexPoly result = new();
            result.ZeroCoefficients(Order + z.Order);

            // Now apply the shift and multiply algorithm

            for (int i = 0; i <= Order; i++)
                for (int j = 0; j <= z.Order; j++)
                    result.Coefficients[i + j] += Coefficients[i] * z.Coefficients[j];

            return result;
        }

        private void NormaliseCoefficients()
        {
            for (int i = Order; i >= 0; i--)
                if (Coefficients[i] == Complex.Zero)
                    Coefficients.RemoveAt(i);
                else
                    return;
        }

        public ComplexPoly Clone()
        {
            ComplexPoly result = new();
            for (int i = 0; i <= Order; i++)
                result.Coefficients.Add(Coefficients[i]);
            return result;
        }

        public ComplexPoly Add(ComplexPoly z)
        {
            ComplexPoly result = new();
            result.ZeroCoefficients(Math.Max(Order, z.Order));
            for (int i = 0; i <= Order; i++)
                result.Coefficients[i] = Coefficients[i];
            for (int i = 0; i <= z.Order; i++)
                result.Coefficients[i] += z.Coefficients[i];
            result.NormaliseCoefficients();
            return result;
        }

        public ComplexPoly Sub(ComplexPoly z)
        {
            ComplexPoly result = new();
            result.ZeroCoefficients(Math.Max(Order, z.Order));
            for (int i = 0; i <= Order; i++)
                result.Coefficients[i] = Coefficients[i];
            for (int i = 0; i <= z.Order; i++)
                result.Coefficients[i] -= z.Coefficients[i];
            result.NormaliseCoefficients();
            return result;
        }

        /// <summary>
        /// Calculate the value of the polynomial at a particular point
        /// </summary>
        /// <param name="z">The point on the polynomial curve we
        /// want to calculate the value for</param>
        /// <returns>The value of the polynomial at the specified
        /// point</returns>

        public Complex Value(Complex z)
        {
            var zPower = Complex.One;
            var result = Complex.Zero;
            foreach (Complex coeff in Coefficients)
            {
                result += zPower * coeff;
                zPower *= z;
            }
            return result;
        }
    }
}
