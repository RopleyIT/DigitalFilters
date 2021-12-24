using DigitalFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalFilterTests
{
    [TestClass]
    public class IIRFilterTests
    {
        [TestMethod]
        public void SecondOrderButterworth()
        {
            Butterworth bw = new(2, 70, false);
            IIRFilter filter = new(bw, 100);
            Assert.AreEqual(0.0808, filter.FilterStages[0].CoeffX[0], 0.0001);
            Assert.AreEqual(0.1616, filter.FilterStages[0].CoeffX[1], 0.0001);
            Assert.AreEqual(0.0808, filter.FilterStages[0].CoeffX[2], 0.0001);
            Assert.AreEqual(1.0509, filter.FilterStages[0].CoeffY[0], 0.0001);
            Assert.AreEqual(-0.3741, filter.FilterStages[0].CoeffY[1], 0.0001);
        }

        [TestMethod]
        public void FirstOrderButterworth()
        {
            Butterworth bw = new(1, 70, false);
            IIRFilter filter = new(bw, 100);
            Assert.AreEqual(0.2674, filter.FilterStages[0].CoeffX[0], 0.0001);
            Assert.AreEqual(0.2674, filter.FilterStages[0].CoeffX[1], 0.0001);
            Assert.AreEqual(0.4652, filter.FilterStages[0].CoeffY[0], 0.0001);
        }
    }
}
