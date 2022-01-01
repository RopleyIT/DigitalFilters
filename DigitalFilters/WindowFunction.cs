using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalFilters
{
    public static class WindowFunction
    {
        private static double ApplyWindow
            (int sample, int maxSamples, double a0, double a1, double a2 = 0, double a3 = 0)
        {
            a0 -= a1 * Math.Cos(2 * Math.PI * sample / (double)maxSamples);
            if(a2 != 0)
                a0 += a2 * Math.Cos(4 * Math.PI * sample / (double)maxSamples);
            if(a3 != 0)
                a0 -= a3 * Math.Cos(6 * Math.PI * sample/ (double)maxSamples);
            return a0;
        }

        public static double Hann(int sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.5, 0.5);

        public static double Hamming(int sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.53836, 0.46164);

        public static double Blackman(int sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.42, 0.5, 0.08);

        public static double ExactBlackman(int sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.42659, 0.49656, 0.076849);

        public static double Nuttall(int sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.355768, 0.487396, 0.144232, 0.012604);

        public static double BlackmanNuttall(int sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.3635819, 0.4891775, 0.1365995, 0.0106411);

        public static double BlackmanHarris(int sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.35875, 0.48829, 0.14128, 0.01168);
    }
}
