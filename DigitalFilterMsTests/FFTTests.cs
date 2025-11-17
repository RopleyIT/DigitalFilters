using DigitalFilters;
using System.Numerics;
namespace DigitalFilterMsTests;

[TestClass]
public class FFTTests
{
    [TestMethod]
    public void TestSineWave()
    {
        FastFourierTransform fft = new(512);
        double[] samples = new double[1024];
        for (int i = 0; i < 1024; i++)
            samples[i] = 100 * Math.Sin(2 * Math.PI * i / 1024.0);
        Complex[] outputSamples = fft.ForwardTransform(samples);
        Assert.AreEqual(-51200, outputSamples[1].Imaginary, 0.00001);
        Assert.AreEqual(0, outputSamples[1].Real, 0.00001);
        Assert.AreEqual(0, outputSamples[0].Real, 0.00001);
        Assert.AreEqual(0, outputSamples[0].Imaginary, 0.00001);
        Assert.IsTrue(!outputSamples.Skip(2).Take(511)
            .Any(s => Math.Abs(s.Imaginary) > 0.00001
            || Math.Abs(s.Real) > 0.00001));
    }

    [TestMethod]
    public void SimpleForwardTransform()
    {
        FastFourierTransform fft = new(8);
        double[] samples = new double[16];
        for (int i = 0; i < 16; i++)
            samples[i] = Math.Sin(Math.PI * i / 8.0);
        Complex[] outputSamples = fft.ForwardTransform(samples);
        Assert.AreEqual(9, outputSamples.Length);
        Assert.AreEqual(-8, outputSamples[1].Imaginary, 0.00001);
        Assert.AreEqual(0, outputSamples[2].Real, 0.00001);
        Assert.AreEqual(0, outputSamples[0].Real, 0.00001);
        Assert.AreEqual(0, outputSamples[0].Imaginary, 0.00001);
        Assert.IsTrue(!outputSamples.Skip(2).Take(7)
            .Any(s => Math.Abs(s.Imaginary) > 0.00001
            || Math.Abs(s.Real) > 0.00001));
    }

    [TestMethod]
    public void SimpleInverseTransform()
    {
        FastFourierTransform fft = new(16);
        Complex[] fSamples = new Complex[9];
        fSamples[1] = new(0, -8); // Sine wave, amplitude 1.0
        double[] outputSamples = fft.InverseTransform(fSamples);
        Assert.AreEqual(16, outputSamples.Length);
        for (int i = 0; i < 16; i++)
            Assert.AreEqual(Math.Sin(Math.PI * i / 8.0), outputSamples[i], 0.0001);
    }

    [TestMethod]
    public void DCValue()
    {
        FastFourierTransform fft = new(512);
        double[] samples = new double[1024];
        for (int i = 0; i < 1024; i++)
            samples[i] = 100;
        Complex[] outputSamples = fft.ForwardTransform(samples);
        Assert.AreEqual(102400, outputSamples[0].Real, 0.00001);
        Assert.AreEqual(0, outputSamples[2].Imaginary, 0.00001);
        Assert.IsTrue(!outputSamples.Skip(1).Take(511)
            .Any(s => Math.Abs(s.Imaginary) > 0.00001
            || Math.Abs(s.Real) > 0.00001));
    }

    [TestMethod]
    public void EachWayTransform()
    {
        FastFourierTransform fft = new(512);
        double[] samples = new double[1024];
        for (int i = 0; i < 1024; i++)
            samples[i] = 100 * Math.Sin(2 * Math.PI * i / 1024.0);
        Complex[] outputSamples = fft.ForwardTransform(samples);
        Assert.AreEqual(513, outputSamples.Length);
        fft = new(1024);
        double[] inverseSamples = fft.InverseTransform(outputSamples);
        Assert.AreEqual(1024, inverseSamples.Length);
        for (int i = 0; i < 1024; i++)
            Assert.AreEqual(samples[i], inverseSamples[i], 0.00001);
    }

    [TestMethod]
    public void StandardFFTWhiteNoise()
    {
        FastFourierTransform fft = new(1024);
        Complex[] samples = new Complex[1024];
        var noiseSource = SignalSources.WhiteNoise(2048, 1.0);
        var iterator = noiseSource.GetEnumerator();
        for (int i = 1; i < 1024; i++)
        {
            iterator.MoveNext();
            var real = iterator.Current;
            iterator.MoveNext();
            var imaginary = iterator.Current;
            samples[i] = new(real, imaginary);
        }
        Complex[] outputSamples = fft.Transform(samples, false);
        Assert.AreEqual(1024, outputSamples.Length);
        Complex[] inverseSamples = fft.Transform(outputSamples, true);
        for (int i = 0; i < 1024; i++)
        {
            Assert.AreEqual(samples[i].Real, inverseSamples[i].Real, 0.00001);
            Assert.AreEqual(samples[i].Imaginary, inverseSamples[i].Imaginary, 0.00001);
        }
    }

    [TestMethod]
    public void SmallWhiteNoise()
    {
        FastFourierTransform fft = new(16);
        var noiseSource = SignalSources.WhiteNoise(32, 1.0);
        double[] samples = noiseSource.ToArray();
        Complex[] outputSamples = fft.ForwardTransform(samples);
        Assert.AreEqual(17, outputSamples.Length);
        fft = new(32);
        double[] inverseSamples = fft.InverseTransform(outputSamples);
        for (int i = 0; i < 32; i++)
            Assert.AreEqual(samples[i], inverseSamples[i], 0.00001);
    }
}
