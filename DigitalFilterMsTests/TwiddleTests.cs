using DigitalFilters;
using System.Numerics;

namespace DigitalFilterMsTests;

[TestClass]
public class TwiddleTests
{
    [TestMethod]
    public void TestSmallTwiddles()
    {
        TwiddleFactors tf = new(16);
        Complex value = tf.Twiddle(0, 8);
        Assert.AreEqual(16, tf.Resolution);
        Assert.AreEqual(1, value.Real);
        Assert.AreEqual(0, value.Imaginary, 1E-10);
    }

    [TestMethod]
    public void TwiddleMidAngles()
    {
        TwiddleFactors tf = new(32);
        Complex value = tf.Twiddle(7, 32);
        Assert.AreEqual(0.1951, value.Real, 0.0001);
        Assert.AreEqual(-0.9808, value.Imaginary, 0.0001);
    }

    [TestMethod]
    public void TwiddleEdgeAngles()
    {
        TwiddleFactors tf = new(32);
        Complex value = tf.Twiddle(31, 32);
        Assert.AreEqual(0.9808, value.Real, 0.0001);
        Assert.AreEqual(0.1951, value.Imaginary, 0.0001);
        value = tf.Twiddle(0, 32);
        Assert.AreEqual(1, value.Real);
        Assert.AreEqual(0, value.Imaginary, 0.0001);
        value = tf.Twiddle(16, 32);
        Assert.AreEqual(-1, value.Real);
        Assert.AreEqual(0, value.Imaginary, 0.0001);
    }

    [TestMethod]
    public void InverseTwiddles()
    {
        TwiddleFactors tf = new(32);
        Complex value = tf.Twiddle(-31, 32);
        Assert.AreEqual(0.9808, value.Real, 0.0001);
        Assert.AreEqual(-0.1951, value.Imaginary, 0.0001);
        value = tf.Twiddle(32, 32);
        Assert.AreEqual(1, value.Real);
        Assert.AreEqual(0, value.Imaginary, 0.0001);
        value = tf.Twiddle(-16, 32);
        Assert.AreEqual(-1, value.Real);
        Assert.AreEqual(0, value.Imaginary, 0.0001);
    }

    [TestMethod]
    public void TwiddleScaling()
    {
        TwiddleFactors tf = new(256);
        Complex value = tf.Twiddle(7, 8);
        Assert.AreEqual(0.7071, value.Real, 0.0001);
        Assert.AreEqual(0.7071, value.Imaginary, 0.0001);
        value = tf.Twiddle(14, 16);
        Assert.AreEqual(0.7071, value.Real, 0.0001);
        Assert.AreEqual(0.7071, value.Imaginary, 0.0001);
        value = tf.Twiddle(224, 256);
        Assert.AreEqual(0.7071, value.Real, 0.0001);
        Assert.AreEqual(0.7071, value.Imaginary, 0.0001);
    }
}
