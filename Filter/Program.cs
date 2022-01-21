using DigitalFilters;
using SvgPlotter;
using System.Drawing;
using System.Numerics;

List<PointF> polarPts = new List<PointF>();
polarPts.Add(new PointF(0, -4));
polarPts.Add(new PointF(0.5F, 0));
polarPts.Add(new PointF(1, 3));
polarPts.Add(new PointF(1.5F, 4));
polarPts.Add(new PointF(2, 3));
polarPts.Add(new PointF(2.5F, 2.5F));
polarPts.Add(new PointF(3, 2));
List<List<PointF>> ptSets = new List<List<PointF>>();
ptSets.Add(polarPts);
string svgPolarPlot = SvgGraph.PlotPolarGraphs(ptSets, 1920, 1080);
using StreamWriter spw = new("C:\\tmp\\polar.svg");
spw.Write(svgPolarPlot);
spw.Close();

List<double> input = SignalSources.SyntheticNoise(65536).ToList();
Butterworth bw = new(7, 2 * Math.PI * 400, false);
Butterworth hw = new(7, 2 * Math.PI * 100, true);
IIRFilter iir = new(bw, 2000);
IIRFilter hir = new(hw, 2000);
double[] output = hir.Filter(iir.Filter(input)).ToArray();
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