using DigitalFilters;
using SvgPlotter;
using System.Drawing;

const int whichPulse = 5;

List<double> input = SignalSources.RaisedCosine(256, 2048, 1000).ToList();
Butterworth bw = new(2, 2 * Math.PI * 9, false);
Butterworth hw = new(2, 2 * Math.PI * 7, true);
IIRFilter iir = new(bw, 2048, 1.7);
IIRFilter hir = new(hw, 2048);
double[] output = input.ToArray();

List<List<PointF>> plots = new();
List<PointF> points = new(input.Count);
points.AddRange(input.Select((p, i) => new PointF(i / (float)2048, (float)p)));
plots.Add(points);
for (int i = 0; i < 6; i++)
{
    output = iir.Filter(output).ToArray();
    output = hir.Filter(output).ToArray();
    if (i == whichPulse)
    {
        points = new(output.Length);
        points.AddRange(output.Select((p, i) => new PointF(i / (float)2048, (float)p)));
        plots.Add(points);
    }
}

Image plot = Plot.PlotGraphs(plots, 3*1920, 1080);
plot.Save($"C:\\tmp\\spectrum{whichPulse}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
string svgPlot = SvgGraph.PlotGraphs(plots, 3*1920, 1080);
using StreamWriter sw = new($"C:\\tmp\\spectrum{whichPulse}.svg");
sw.Write(svgPlot);
sw.Close();