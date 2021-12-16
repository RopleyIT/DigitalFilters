using DigitalFilters;
using System.Drawing;
using SvgPlotter;

IEnumerable<double> Impulse(int width, int duration, int magnitude = 1)
{
    for (int i = 0; i < width; i++)
        yield return magnitude;
    for (int i = width; i < duration; i++)
        yield return 0.0;
}

IEnumerable<double> SineWave(int frequency, int sampleRate, int duration, int magnitude = 1)
{
    for(int i = 0; i < duration; i++)
        yield return magnitude * Math.Sin(2*i*Math.PI*frequency/sampleRate);
}

IEnumerable<double> RaisedCosine(int width, int duration, int magnitude = 1)
{
    for (int i = 0; i < width; i++)
        yield return magnitude * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (double)width));
    for (int i = width; i < duration; i++)
        yield return 0;
}

List<double> ApplyFilter(IIRFilter filter, List<double> source)
{
    List<double> result = new(source.Count);
    result = filter.Filter(source).ToList();
    return result;
}
List<double> samples = RaisedCosine(20, 500, 100).ToList();
//List<double> samples = SineWave(75, 4000, 500, 50).ToList();
//List<double> samples = Impulse(250, 500, 100).ToList();
Butterworth bw = new(7, 2 * Math.PI * 100, false);
Butterworth hw = new(7, 2 * Math.PI * 75, true);
IIRFilter iir = new(bw, 4000);
IIRFilter hir = new(hw, 4000);
List<List<PointF>> plots = new();
plots.Add(samples.Select((p, i) => new PointF(i, (float)p)).ToList());
for(int k = 0; k < 5; k++)
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
