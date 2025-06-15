using DigitalFilters;
using Plotter;
using TwoDimensionLib;
using Interpolators;
using WavFileManager;

// IIR filter demo

const int whichPulse = 5;

List<double> input = SignalSources.RaisedCosine(256, 2048, 1000).ToList();
Butterworth bw = new(2, 2 * Math.PI * 9, false);
Butterworth hw = new(2, 2 * Math.PI * 7, true);
IIRFilter iir = new(bw, 2048, 1.7);
IIRFilter hir = new(hw, 2048);
double[] output = input.ToArray();

List<List<Coordinate>> plots = new();
List<Coordinate> points = new(input.Count);
points.AddRange(input.Select((p, i) => new Coordinate(i / 2048.0, p)));
plots.Add(points);
for (int i = 0; i < 6; i++)
{
    output = iir.Filter(output).ToArray();
    output = hir.Filter(output).ToArray();
    if (i == whichPulse)
    {
        points = new(output.Length);
        points.AddRange(output.Select((p, i) => new Coordinate(i / 2048.0, p)));
        plots.Add(points);
    }
}

string svgPlot = SVGPlot.PlotGraphs(plots, 3*1920, 1080);
using StreamWriter sw = new($"C:\\tmp\\spectrum{whichPulse}.svg");
sw.Write(svgPlot);
sw.Close();

// Pitch shifting demo

input = SignalSources.SineWave(261, 0, 44100, 88200).ToList();
PitchShifter ps = new(input, 1/1.4983, 32);
ps.WindowFunction = WindowFunction.Hann;
output = ps.ShiftedSamples().ToArray();

plots = new();
points = new(input.Count);
points.AddRange(input.Take(882).Select((p, i) => new Coordinate(i, p)));
plots.Add(points);
points = new(output.Length);
points.AddRange(output.Take(882).Select((p, i) => new Coordinate(i, p)));
plots.Add(points);
svgPlot = SVGPlot.PlotGraphs(plots, 2 * 1920, 1080);
using StreamWriter sps = new($"C:\\tmp\\shift.svg");
sps.Write(svgPlot);
sps.Close();

// .WAV file writing demo

WavWriter w = new WavWriter("C:\\tmp\\shift.wav", 44100, 1, 32);
foreach(double v in input)
    w.WriteMono(v);
foreach (double v in output)
    w.WriteMono(v);
w.Flush();