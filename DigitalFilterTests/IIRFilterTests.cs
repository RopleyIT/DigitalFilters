using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigitalFilters;
using System.Numerics;
using System;

namespace DigitalFilterTests
{
    [TestClass]
    public class IIRFilterTests
    {
        [TestMethod]
        public void SecondOrderButterworth()
        {
            Butterworth bw = new(2, 70, false);
            IIRFilter filter = new IIRFilter(bw.Polynomials[0], 70, 100, false);
            Assert.AreEqual(0.0808, filter.CoeffX[0], 0.0001);
            Assert.AreEqual(0.1616, filter.CoeffX[1], 0.0001);
            Assert.AreEqual(0.0808, filter.CoeffX[2], 0.0001);
            Assert.AreEqual(1.0509, filter.CoeffY[0], 0.0001);
            Assert.AreEqual(-0.3741, filter.CoeffY[1], 0.0001);
        }

        [TestMethod]
        public void FirstOrderButterworth()
        {
            Butterworth bw = new(1, 70, false);
            IIRFilter filter = new IIRFilter(bw.Polynomials[0], 70, 100, false);
            Assert.AreEqual(0.2674, filter.CoeffX[0], 0.0001);
            Assert.AreEqual(0.2674, filter.CoeffX[1], 0.0001);
            Assert.AreEqual(0.4652, filter.CoeffY[0], 0.0001);
        }
    }
}
