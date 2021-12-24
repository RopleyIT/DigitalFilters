using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalFilters
{
    /// <summary>
    /// Generate digitally samples waveforms of various shapes
    /// </summary>
    
    public static class SignalSources
    {
        public static IEnumerable<double> Impulse(int width, int duration, int magnitude = 1)
        {
            for (int i = 0; i < width; i++)
                yield return magnitude;
            for (int i = width; i < duration; i++)
                yield return 0.0;
        }

        public static IEnumerable<double> SineWave(int frequency, int sampleRate, int duration, int magnitude = 1)
        {
            for (int i = 0; i < duration; i++)
                yield return magnitude * Math.Sin(2 * i * Math.PI * frequency / sampleRate);
        }

        public static IEnumerable<double> RaisedCosine(int width, int duration, int magnitude = 1)
        {
            for (int i = 0; i < width; i++)
                yield return magnitude * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (double)width));
            for (int i = width; i < duration; i++)
                yield return 0;
        }

        public static IEnumerable<double> WhiteNoise(int sampleCount, double gain = 1.0)
        {
            for (int i = 0; i < sampleCount; i++)
                yield return Gaussian() * gain;
        }

        static readonly Random random = new();
        public static double Gaussian()
        {
            double v = 0;
            for (int i = 0; i < 8192; i++)
                v += 2 * random.NextDouble() - 1;
            return v;
        }

    }
}
