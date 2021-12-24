using DigitalFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace DigitalFilterTests
{
    [TestClass]
    public class ButterworthTests
    {
        [TestMethod]
        public void FirstOrderPolynomialsCorrect()
        {
            Butterworth bw = new(1, 1, false);
            Assert.AreEqual(1, bw.Polynomials.Count);
            Assert.AreEqual(2, bw.Polynomials[0].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[0].Real);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[0].Imaginary);
        }

        [TestMethod]
        public void SecondOrderPolynomialsCorrect()
        {
            Butterworth bw = new(2, 1, false);
            Assert.AreEqual(1, bw.Polynomials.Count);
            Assert.AreEqual(3, bw.Polynomials[0].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[0].Real);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[0].Imaginary);
            Assert.AreEqual(1.414, bw.Polynomials[0].Coefficients[1].Real, 0.001);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[1].Imaginary, 0.001);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[2].Real);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[2].Imaginary);
        }

        [TestMethod]
        public void ThirdOrderPolynomialsCorrect()
        {
            Butterworth bw = new(3, 1, false);
            Assert.AreEqual(2, bw.Polynomials.Count);
            Assert.AreEqual(2, bw.Polynomials[1].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[1].Coefficients[0].Real);
            Assert.AreEqual(0.0, bw.Polynomials[1].Coefficients[0].Imaginary);
            Assert.AreEqual(3, bw.Polynomials[0].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[0].Real, 0.001);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[0].Imaginary, 0.001);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[1].Real, 0.001);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[1].Imaginary, 0.001);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[2].Real, 0.001);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[2].Imaginary, 0.001);
        }

        [TestMethod]
        public void TenthOrderPolynomialsCorrect()
        {
            Butterworth bw = new(10, 1, false);
            Assert.AreEqual(5, bw.Polynomials.Count);
            Assert.AreEqual(3, bw.Polynomials[0].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[0].Real);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[0].Imaginary);
            Assert.AreEqual(0.312869, bw.Polynomials[0].Coefficients[1].Real, 0.000001);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[1].Imaginary, 0.001);
            Assert.AreEqual(1.0, bw.Polynomials[0].Coefficients[2].Real);
            Assert.AreEqual(0.0, bw.Polynomials[0].Coefficients[2].Imaginary);
            Assert.AreEqual(3, bw.Polynomials[4].Coefficients.Count);
            Assert.AreEqual(1.0, bw.Polynomials[4].Coefficients[0].Real, 0.001);
            Assert.AreEqual(0.0, bw.Polynomials[4].Coefficients[0].Imaginary);
            Assert.AreEqual(1.975377, bw.Polynomials[4].Coefficients[1].Real, 0.000001);
            Assert.AreEqual(0.0, bw.Polynomials[4].Coefficients[1].Imaginary, 0.001);
            Assert.AreEqual(1.0, bw.Polynomials[4].Coefficients[2].Real);
            Assert.AreEqual(0.0, bw.Polynomials[4].Coefficients[2].Imaginary);
        }

        [TestMethod]
        public void DCGainCorrect()
        {
            Butterworth bw = new(9, 1, false);
            Complex v = bw.OutputAtFrequency(0);
            Assert.AreEqual(1.0, v.Real, 0.001);
            Assert.AreEqual(0.0, v.Imaginary, 0.001);
        }

        [TestMethod]
        public void DCGainCorrectAtFreq()
        {
            Butterworth bw = new(9, 1000 * Math.PI, false); // 500Hz cutoff
            Complex v = bw.OutputAtFrequency(0);
            Assert.AreEqual(1.0, v.Real, 0.001);
            Assert.AreEqual(0.0, v.Imaginary, 0.001);
        }

        [TestMethod]
        public void GainAtUnitCutoffCorrect()
        {
            Butterworth bw = new(7, 1, false);
            Complex v = bw.OutputAtFrequency(1);
            Assert.AreEqual(1.414, v.Magnitude, 0.001);
        }

        [TestMethod]
        public void GainAtCutoffCorrect()
        {
            Butterworth bw = new(7, 1000 * Math.PI, false);
            Complex v = bw.OutputAtFrequency(1000 * Math.PI);
            Assert.AreEqual(1.414, v.Magnitude, 0.001);
        }
    }
}