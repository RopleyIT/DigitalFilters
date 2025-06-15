using DigitalFilters;
using Plotter;
using TwoDimensionLib;
using System.Numerics;

List<Coordinate> polarPts = new ();
polarPts.Add(new Coordinate(0, -4));
polarPts.Add(new Coordinate(0.5F, 0));
polarPts.Add(new Coordinate(1, 3));
polarPts.Add(new Coordinate(1.5F, 4));
polarPts.Add(new Coordinate(2, 3));
polarPts.Add(new Coordinate(2.5F, 2.5F));
polarPts.Add(new Coordinate(3, 2));
List<List<Coordinate>> ptSets = new ();
ptSets.Add(polarPts);

string svgPolarPlot = SVGPlot.PlotPolarGraphs(ptSets, 1920, 1080);
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

List<List<Coordinate>> plots = new();
List<Coordinate> points = new(results.Length);
points.AddRange(results.Select((p, i) => new Coordinate(i/32.768, p.Magnitude)));
plots.Add(points);

string svgPlot = SVGPlot.PlotGraphs(plots, 1920, 1080);
using StreamWriter sw = new("C:\\tmp\\spectrum.svg");
sw.Write(svgPlot);
sw.Close();