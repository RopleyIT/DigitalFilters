using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigitalFilters;
using System.Numerics;
using System;
using System.Linq;
namespace DigitalFilterTests
{
    [TestClass]
    public class FFTTests
    {
        [TestMethod]
        public void TestSineWave()
        {
            FastFourierTransform fft = new(10);
            double[] samples = new double[1024];
            for (int i = 0; i < 1024; i++)
                samples[i] = 100 * Math.Sin(2 * Math.PI * i / 1024.0);
            Complex[] outputSamples = fft.Transform(samples);
            Assert.AreEqual(-51200, outputSamples[1].Imaginary, 0.00001);
            Assert.AreEqual(0, outputSamples[1].Real, 0.00001);
            Assert.AreEqual(0, outputSamples[0].Real, 0.00001);
            Assert.AreEqual(0, outputSamples[0].Imaginary, 0.00001);
            Assert.IsTrue(!outputSamples.Skip(2).Take(511)
                .Any(s => Math.Abs(s.Imaginary) > 0.00001
                || Math.Abs(s.Real) > 0.00001));
        }

        [TestMethod]
        public void SmallFFT()
        {
            FastFourierTransform fft = new(4);
            double[] samples = new double[16];
            for (int i = 0; i < 16; i++)
                samples[i] = 100 * Math.Sin(2 * Math.PI * i / 16.0);
            Complex[] outputSamples = fft.Transform(samples);
            Assert.AreEqual(-800, outputSamples[1].Imaginary, 0.00001);
            Assert.AreEqual(0, outputSamples[2].Real, 0.00001);
            Assert.AreEqual(0, outputSamples[0].Real, 0.00001);
            Assert.AreEqual(0, outputSamples[0].Imaginary, 0.00001);
            Assert.IsTrue(!outputSamples.Skip(2).Take(7)
                .Any(s => Math.Abs(s.Imaginary) > 0.00001
                || Math.Abs(s.Real) > 0.00001));
        }

        [TestMethod]
        public void DCValue()
        {
            FastFourierTransform fft = new(10);
            double[] samples = new double[1024];
            for (int i = 0; i < 1024; i++)
                samples[i] = 100;
            Complex[] outputSamples = fft.Transform(samples);
            Assert.AreEqual(102400, outputSamples[0].Real, 0.00001);
            Assert.AreEqual(0, outputSamples[2].Imaginary, 0.00001);
            Assert.IsTrue(!outputSamples.Skip(1).Take(511)
                .Any(s => Math.Abs(s.Imaginary) > 0.00001
                || Math.Abs(s.Real) > 0.00001));
        }

        [TestMethod]
        public void InverseTransform()
        {
            FastFourierTransform fft = new(10);
            double[] samples = new double[1024];
            for (int i = 0; i < 1024; i++)
                samples[i] = 100 * Math.Sin(2 * Math.PI * i / 1024.0);
            Complex[] outputSamples = fft.Transform(samples);
            Complex[] inverseSamples = fft.Transform(outputSamples, true);
            for (int i = 0; i < 1024; i++)
            {
                Assert.AreEqual(samples[i], inverseSamples[i].Real, 0.00001);
                Assert.AreEqual(0, inverseSamples[i].Imaginary, 0.00001);
            }
        }
    }
}
