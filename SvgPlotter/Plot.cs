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

    private static float ScalePolar(RectangleF bounds, int width, int height)
    {
        if (width < height)
            return width / (2 * bounds.Height);
        else
            return height / (2 * bounds.Height);
    }

    public static Image PlotGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height, Color? color = null)
    {
        Color[] colours = { Color.Black, Color.Brown, Color.Red, Color.DarkBlue,
                Color.Green, Color.Magenta, Color.Cyan, Color.Gray};
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

    public static Image PlotPolarGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height, Color? color = null)
    {
        Color[] colours = { Color.Black, Color.Brown, Color.Red, Color.DarkBlue,
                Color.Green, Color.Magenta, Color.Cyan, Color.Gray};
        if (color.HasValue)
            colours = new Color[] { color.Value };

        Bitmap bmp = new(width, height);
        BoundsF bounds = new();
        List<List<PointF>> plots = new();
        foreach (IEnumerable<PointF> pl in points)
            plots.Add(bounds.Track(pl).ToList());
        float scale = ScalePolar(bounds.Bounds, width, height);

        using Graphics g = Graphics.FromImage(bmp);
        g.FillRectangle(Brushes.White, 0, 0, width, height);
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        PlotPolarAxes(g, bounds, scale);
        int index = 0;
        foreach (List<PointF> pl in plots)
            PlotPolarGraph(pl, g, bounds.Bounds, scale, colours[index++ % colours.Length]);
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
            LabelXRule(g, v, bounds, scale);
        }
        for (double v = RoundUp(bounds.Bounds.Y, unitsY); v < bounds.Bounds.Bottom; v += unitsY)
        {
            List<PointF> rule = new()
            {
                new PointF { Y = (float)v, X = bounds.Bounds.X },
                new PointF { Y = (float)v, X = bounds.Bounds.Right }
            };
            PlotGraph(rule, g, bounds.Bounds, scale, Color.Gray);
            LabelYRule(g, v, bounds, scale);
        }
    }

    private static void PlotPolarAxes(Graphics g, BoundsF bounds, float scale)
    {
        double unitsθ = Math.PI / 6;
        double unitsR = UnitSize(bounds.Bounds.Height);
        Pen p = new (Color.Gray, 1);
        PointF origin = TransformPolar(new PointF(0, bounds.Bounds.Y), bounds.Bounds, scale);
        for (int i = -6; i < 6; i++)
        {
            PointF end = TransformPolar
                (new PointF((float)(i * unitsθ), bounds.Bounds.Bottom), bounds.Bounds, scale);
            g.DrawLine(p, origin, end);
            if (i != 0)
                LabelPoint(g, i * 30, end);
        }

        for (double v = RoundUp(bounds.Bounds.Y, unitsR); v <= bounds.Bounds.Bottom; v += unitsR)
        {
            float radius = (float)((v - bounds.Bounds.Y) * scale);
            RectangleF circleBounds = new ()
            {
                X = origin.X - radius,
                Y = origin.Y - radius,
                Width = 2 * radius,
                Height = 2 * radius
            };
            g.DrawEllipse(p, circleBounds);
            LabelRRule(g, v, bounds, scale);
        }
    }

    private static void LabelXRule(Graphics g, double v, BoundsF bounds, SizeF scale)
    {
        PointF txtLoc = TransformPt(new PointF((float)v, 0), bounds.Bounds, scale);
        LabelPoint(g, v, txtLoc);
    }

    private static void LabelYRule(Graphics g, double v, BoundsF bounds, SizeF scale)
    {
        PointF txtLoc = TransformPt(new PointF(0, (float)v), bounds.Bounds, scale);
        LabelPoint(g, v, txtLoc);
    }

    private static void LabelRRule(Graphics g, double v, BoundsF bounds, float scale)
    {
        PointF txtLoc = TransformPolar(new PointF(0, (float)v), bounds.Bounds, scale);
        LabelPoint(g, v, txtLoc);
    }

    private static void LabelPoint(Graphics g, double v, PointF location)
        => LabelPoint(g, v.ToString("G3"), location);

    private static void LabelPoint(Graphics g, string label, PointF location)
    {
        Font font = new("Consolas", 20F);
        SizeF txtSize = g.MeasureString(label, font);
        //location.X += txtSize.Height / 2; 
        //location.Y -= txtSize.Height / 2;
        location.Y -= txtSize.Height;
        //g.FillRectangle(Brushes.White, location.X, location.Y, txtSize.Width, txtSize.Height);
        g.DrawString(label, font, Brushes.Gray, location.X, location.Y);
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

    public static Image PlotGraph(IEnumerable<PointF> points, int width, int height, Color? color = null)
    {
        List<IEnumerable<PointF>> pointLists = new() { points };
        return PlotGraphs(pointLists, width, height, color);
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

    private static void PlotPolarGraph(List<PointF> points, Graphics g,
        RectangleF bounds, float scale, Color penColor)
    {
        Pen p = new (penColor, 2);
        var transformedPoints = points
            .Select(pt => TransformPolar(pt, bounds, scale))
            .ToArray();
        g.DrawLines(p, transformedPoints);
    }

    private static PointF TransformPt(PointF p, RectangleF bounds, SizeF scale)
    => new((float)(scale.Width * (p.X - bounds.X)),
        (float)(scale.Height * (bounds.Height - p.Y + bounds.Y)));
    
    private static PointF TransformPolar(PointF p, RectangleF bounds, float scale)
    {
        float radius = scale * (p.Y - bounds.Y);
        float maxRadius = bounds.Height * scale;
        float angle = p.X;
        return new PointF((float)(maxRadius + radius * Math.Cos(angle)), (float)(maxRadius - radius * Math.Sin(angle)));
    }
}
