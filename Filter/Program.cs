using DigitalFilters;
using SvgPlotter;
using System.Drawing;
using System.Numerics;

static List<double> ApplyFilter(IIRFilter filter, List<double> source)
{
    List<double> result = new(source.Count);
    result = filter.Filter(source).ToList();
    return result;
}
/*
List<double> samples = SignalSources.RaisedCosine(20, 500, 100).ToList();
//List<double> samples = SignalSources.SineWave(75, 4000, 500, 50).ToList();
//List<double> samples = SignalSources.Impulse(250, 500, 100).ToList();
Butterworth bw = new(7, 2 * Math.PI * 100, false);
Butterworth hw = new(7, 2 * Math.PI * 75, true);
IIRFilter iir = new(bw, 4000);
IIRFilter hir = new(hw, 4000);
List<List<PointF>> plots = new();
plots.Add(samples.Select((p, i) => new PointF(i, (float)p)).ToList());
for (int k = 0; k < 5; k++)
{
    //samples = ApplyFilter(iir, samples);
    samples = ApplyFilter(hir, samples);
    List<PointF> points = new(samples.Count);
    points.AddRange(samples.Select((p, i) => new PointF(i, (float)p)));
    plots.Add(points);
}

Image plot = Plot.PlotGraphs(plots, 1920, 1080);
plot.Save("C:\\tmp\\filter.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
string svgPlot = SvgGraph.PlotGraphs(plots, 1920, 1080);
using StreamWriter sw = new("C:\\tmp\\filter.svg");
sw.Write(svgPlot);
sw.Close();
*/

List<double> input = SignalSources.WhiteNoise(65536, 0.6).ToList();
Butterworth bw = new(7, 2 * Math.PI * 400, false);
Butterworth hw = new(7, 2 * Math.PI * 100, true);
IIRFilter iir = new(bw, 2000);
IIRFilter hir = new(hw, 2000);
double[] output = ApplyFilter(hir, ApplyFilter(iir, input)).ToArray();
FastFourierTransform fft = new(32768);
Complex[] results = fft.ForwardTransform(output);

List<List<PointF>> plots = new();
List<PointF> points = new(results.Length);
points.AddRange(results.Select((p, i) => new PointF(i/(float)32.768, (float)(p.Magnitude))));
plots.Add(points);

Image plot = Plot.PlotGraphs(plots, 1920, 1080);
plot.Save("C:\\tmp\\spectrum.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
string svgPlot = SvgGraph.PlotGraphs(plots, 1920, 1080);
using StreamWriter sw = new("C:\\tmp\\spectrum.svg");
sw.Write(svgPlot);
sw.Close();

