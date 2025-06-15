using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalFilters
{
    /// <summary>
    /// Generator for window functions used to reduce stopband
    /// ripples in the frequency response of digital filters.
    /// </summary>
    
    public static class WindowFunction
    {
        /// <summary>
        /// Given a window function's parameters, and the number of samples
        /// the window function is stretched over, calculate the amplitude
        /// of the window function at a given sample offset into the function.
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals. This is of type double so that values
        /// other than at sample intervals can be obtained, as is
        /// useful when interpolating waveforms for pitch shifting.</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals. Note that from edge to edge,
        /// there are maxSamples + 1 smaple points for a symmetrical
        /// window. Hence choosing a value of 4 would give the edge
        /// values at smaples 0 and 4, and the centre peak value at 2.
        /// If an odd value is chosen for maxSamples, the mid-point
        /// of the window function does not align with any sample
        /// point, but the two highest values are either side of half
        /// way through the list of smaples. For example, a maxSamples
        /// value of 5 will have equal max value smaples at smaples
        /// 2 and 3.</param>
        /// <param name="a0">DC component amplitude of window function</param>
        /// <param name="a1">Fundamental one-cycle amplitude</param>
        /// <param name="a2">First harmonic amplitude</param>
        /// <param name="a3">2nd harmonic amplitude</param>
        /// <returns>The window function value for the specified smaple</returns>
        
        private static double ApplyWindow
            (double sample, int maxSamples, double a0, double a1, double a2 = 0, double a3 = 0)
        {
            if (sample < 0 || sample > maxSamples)
                return 0.0;
            a0 -= a1 * Math.Cos(2 * Math.PI * sample / (double)maxSamples);
            if(a2 != 0)
                a0 += a2 * Math.Cos(4 * Math.PI * sample / (double)maxSamples);
            if(a3 != 0)
                a0 -= a3 * Math.Cos(6 * Math.PI * sample/ (double)maxSamples);
            return a0;
        }

        /// <summary>
        /// Window function for a Hann window.
        /// The Hann window is a raised cosine window of width maxSamples 
        /// for one full period.It has a widening of the main lobe 
        /// relative to the unwindowed case of 1.5, but first sidelobes 
        /// at -32dB and roll-off around 18dB per octave.        
        /// </summary>
        /// <param name="sample">The ofset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double Hann(double sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.5, 0.5);

        /// <summary>
        /// Window function for a Hamming window.
        /// This window function slightly alters the Hann window function to 
        /// remove the first sidelobes of the Hann window spectrum. It has its 
        /// highest sidelobes at -43dB, but sidelobes far from the centre of 
        /// the window's spectrum do not fall away at 18dB per octave, but 
        /// settle at around -60dB. The main lobe width is also slightly 
        /// narrower than Hann, at 1.368 times the rectangular window.      
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double Hamming(double sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.53836, 0.46164);

        /// <summary>
        /// Window function for a Blackman window.
        /// By introducing an extra raised cosine component at half the 
        /// period of the Hann and Hamming windows, the Blackman window 
        /// has its first sidelobes at -58dB with an 18dB per octave 
        /// roll-off from there. The penalty paid is the main lobe spread 
        /// is widened to 1.727 times the width of the main lobe for a 
        /// rectangular window.   
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double Blackman(double sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.42, 0.5, 0.08);

        /// <summary>
        /// Window function for an exact Blackman window.
        /// This is an implementation of the Blackman window that uses
        /// the exact values for the optimum coefficients for the
        /// window function. The coefficients for the simple Blackman
        /// window function above are frequently used approximations
        /// that provide slightly degraded performance.
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double ExactBlackman(double sample, int maxSamples)
            => ApplyWindow(sample, maxSamples, 0.42659, 0.49656, 0.076849);

        /// <summary>
        /// Window function for a Nuttall window.
        /// The Nuttall window incorporates a third harmonic term in its sum 
        /// of raised cosine curves. Though this widens the main lobe to 2.021 
        /// times the width of the rectangular window, the highest sidelobes 
        /// appear at -93dB with an 18dB per octave roll-off thereafter.
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double Nuttall(double sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.355768, 0.487396, 0.144232, 0.012604);

        /// <summary>
        /// Window function for a Blackman - Nuttall window.
        /// The Blackman-Nuttall window function slightly tweaks the coefficients
        /// for the Nuttall window function so that the main lobe reduces 
        /// slightly to 1.976 times the rectangular window width, but the 
        /// highest sidelobes are at -98dB.
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double BlackmanNuttall(double sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.3635819, 0.4891775, 0.1365995, 0.0106411);

        /// <summary>
        /// Window function for a Blackman - Harris window.
        /// The Blackman-Harris window function slightly tweaks the coefficients
        /// for the Blackman - Nuttall window function so that the sidelobes
        /// reduce their peaks the further from the passband they are, as
        /// opposed to the Blackman-Nuttall window which has a residual,
        /// shallow-tapering sidelobe height around -98dB.
        /// </summary>
        /// <param name="sample">The offset into the window function as a count
        /// of sample intervals</param>
        /// <param name="maxSamples">The length of the window function
        /// as a multiple of smaple intervals.</param
        /// <returns>The window function value for the specified sample</returns>

        public static double BlackmanHarris(double sample, int maxSamples)
            => ApplyWindow
            (sample, maxSamples, 0.35875, 0.48829, 0.14128, 0.01168);
    }
}
