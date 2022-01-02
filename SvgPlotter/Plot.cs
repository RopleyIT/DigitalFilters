using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SvgPlotter;

public static class Plot
{
    private static SizeF ScaleFactor(RectangleF bounds, int width, int height, bool lockAspectRatio)
    {
        float scaleY = height / bounds.Height;
        float scaleX = width / bounds.Width;
        if (lockAspectRatio)
            scaleY = scaleX = Math.Min(scaleX, scaleY);
        return new SizeF(scaleX, scaleY);
    }

    public static Image PlotGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height, Color? color = null)
    {
        Color[] colours = { Color.Black, Color.Brown, Color.Red, Color.DarkBlue,
                Color.Green, Color.Blue, Color.Purple, Color.Gray};
        if (color.HasValue)
            colours = new Color[] { color.Value };

        Bitmap bmp = new(width, height);
        BoundsF bounds = new();
        List<List<PointF>> plots = new();
        foreach (IEnumerable<PointF> pl in points)
            plots.Add(bounds.Track(pl).ToList());
        SizeF scale = ScaleFactor(bounds.Bounds, width, height, false);
        using Graphics g = Graphics.FromImage(bmp);
        g.FillRectangle(Brushes.White, 0, 0, width, height);
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        PlotAxes(g, bounds, scale);
        int index = 0;
        foreach (List<PointF> pl in plots)
            PlotGraph(pl, g, bounds.Bounds, scale, colours[index++ % colours.Length]);
        return bmp;
    }

    private static void PlotAxes(Graphics g, BoundsF bounds, SizeF scale)
    {
        double unitsX = UnitSize(bounds.Bounds.Width);
        double unitsY = UnitSize(bounds.Bounds.Height);

        for (double v = RoundUp(bounds.Bounds.X, unitsX); v < bounds.Bounds.Right; v += unitsX)
        {
            List<PointF> rule = new()
            {
                new PointF { X = (float)v, Y = bounds.Bounds.Y },
                new PointF { X = (float)v, Y = bounds.Bounds.Bottom }
            };
            PlotGraph(rule, g, bounds.Bounds, scale, Color.Gray);
            LabelXRule(v, g, bounds, scale);
        }
        for (double v = RoundUp(bounds.Bounds.Y, unitsY); v < bounds.Bounds.Bottom; v += unitsY)
        {
            List<PointF> rule = new()
            {
                new PointF { Y = (float)v, X = bounds.Bounds.X },
                new PointF { Y = (float)v, X = bounds.Bounds.Right }
            };
            PlotGraph(rule, g, bounds.Bounds, scale, Color.Gray);
            LabelYRule(v, g, bounds, scale);
        }
    }

    private static void LabelXRule(double v, Graphics g, BoundsF bounds, SizeF scale)
    {
        // First generate label string

        string label = v.ToString("G2");
        Font font = new("Consolas", 20F);
        SizeF txtSize = g.MeasureString(label, font);

        // Find the line position
        PointF txtLoc = TransformPt(new PointF((float)v, 0), bounds.Bounds, scale);
        txtLoc.Y -= txtSize.Height;
        //g.FillRectangle(Brushes.White, txtLoc.X, txtLoc.Y, txtSize.Width, txtSize.Height);
        g.DrawString(label, font, Brushes.Gray, txtLoc.X, txtLoc.Y);
    }

    private static void LabelYRule(double v, Graphics g, BoundsF bounds, SizeF scale)
    {
        // First generate label string

        string label = v.ToString("G3");
        Font font = new("Consolas", 20F);
        SizeF txtSize = g.MeasureString(label, font);

        // Find the line position
        
        PointF txtLoc = TransformPt(new PointF(0, (float)v), bounds.Bounds, scale);
        txtLoc.Y -= txtSize.Height;
        //g.FillRectangle(Brushes.White, txtLoc.X, txtLoc.Y, txtSize.Width, txtSize.Height);
        g.DrawString(label, font, Brushes.Gray, txtLoc.X, txtLoc.Y);
    }

    private static double RoundUp(double x, double unitsX) => Math.Ceiling(x / unitsX) * unitsX;
    private static double UnitSize(double range)
    {
        double log = Math.Log10(range);
        double exponent = Math.Floor(log);
        double baseUnit = Math.Pow(10.0, exponent - 1);
        double mantissa = log - exponent;
        if (mantissa <= Math.Log10(2))
            return 2 * baseUnit;
        if (mantissa <= Math.Log10(5))
            return 5 * baseUnit;
        return 10 * baseUnit;
    }

    public static Image PlotGraph(List<PointF> points, int width, int height)
    {
        List<List<PointF>> pointLists = new() { points };
        return PlotGraphs(pointLists, width, height);
    }

    private static void PlotGraph(List<PointF> points, Graphics g,
        RectangleF bounds, SizeF scale, Color penColor)
    { 
        using Pen p = new(penColor, 2);
        p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
        p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
        p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        var transformedPoints = points
            .Select(pt => TransformPt(pt, bounds, scale))
            .ToArray();
        g.DrawLines(p, transformedPoints);
    }

    private static PointF TransformPt(PointF p, RectangleF bounds, SizeF scale)
    => new((float)(scale.Width * (p.X - bounds.X)),
        (float)(scale.Height * (bounds.Height - p.Y + bounds.Y)));
}
