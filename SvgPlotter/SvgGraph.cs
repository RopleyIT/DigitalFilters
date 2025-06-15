using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;

namespace SvgPlotter
{
    /// <summary>
    /// Plot a set of points on a graph using SVG graphics
    /// </summary>

    public static class SvgGraph
    {
        private static SizeF ScaleFactor(RectangleF bounds, int width, int height, bool lockAspectRatio)
        {
            float scaleY = height / bounds.Height;
            float scaleX = width / bounds.Width;
            if(lockAspectRatio)
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

        public static string PlotGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height, string color = null)
        {
            string[] colours = { "black", "brown", "red", "darkblue",
                "green", "magenta", "cyan", "gray" };
            if (!string.IsNullOrWhiteSpace(color))
                colours = new string[] { color };

            SVGCreator svgImage = new();
            svgImage.DocumentDimensions = new Size(width, height);
            svgImage.ViewBoxDimensions = new RectangleF(0, 0,
                svgImage.DocumentDimensions.Width,
                svgImage.DocumentDimensions.Height);
            BoundsF bounds = new();
            List<List<PointF>> plots = new();
            foreach (IEnumerable<PointF> pl in points)
                plots.Add(bounds.Track(pl).ToList());
            SizeF scale = ScaleFactor(bounds.Bounds, width, height, false);

            PlotAxes(bounds, scale, svgImage);
            int index = 0;
            foreach (List<PointF> pl in plots)
                PlotGraph(pl, svgImage, bounds.Bounds, scale, colours[index++ % colours.Length]);
            return svgImage.ToString();
        }

        public static string PlotPolarGraphs(IEnumerable<IEnumerable<PointF>> points, int width, int height, string color = null)
        {
            string[] colours = { "black", "brown", "red", "darkblue",
                "green", "magenta", "cyan", "gray" };
            if (!string.IsNullOrWhiteSpace(color))
                colours = new string[] { color };

            SVGCreator svgImage = new();
            svgImage.DocumentDimensions = new Size(width, height);
            svgImage.ViewBoxDimensions = new RectangleF(-width/2, -height/2,
                svgImage.DocumentDimensions.Width,
                svgImage.DocumentDimensions.Height);
            BoundsF bounds = new();
            List<List<PointF>> plots = new();
            foreach (IEnumerable<PointF> pl in points)
                plots.Add(bounds.Track(pl).ToList());
            float scale = ScalePolar(bounds.Bounds, width, height);

            PlotPolarAxes(bounds, scale, svgImage);
            int index = 0;
            foreach (List<PointF> pl in plots)
                PlotPolarGraph(pl, svgImage, bounds.Bounds, scale, colours[index++ % colours.Length]);
            return svgImage.ToString();
        }

        private static void PlotPolarAxes(BoundsF bounds, float scale, SVGCreator svgImage)
        {
            double unitsθ = Math.PI / 6;
            double unitsR = UnitSize(bounds.Bounds.Height);
            for(int i = -6; i < 6; i++)
            {
                PointF end = TransformPolar
                        (new PointF((float)(i*unitsθ), bounds.Bounds.Bottom), bounds.Bounds, scale);
                svgImage.AddLine(PointF.Empty, end, "gray", 1);
                if(i != 0)
                    LabelPoint(svgImage, i * 30, end);
            }

            for (double v = RoundUp(bounds.Bounds.Y, unitsR); v <= bounds.Bounds.Bottom; v += unitsR)
            {
                svgImage.AddCircle(PointF.Empty, (float)((v - bounds.Bounds.Y) * scale), "gray", 1, null);
                LabelRRule(v, svgImage, bounds, scale);
            }
        }

        private static void PlotAxes(BoundsF bounds, SizeF scale, SVGCreator svgImage)
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
                PlotGraph(rule, svgImage, bounds.Bounds, scale, "gray");
                LabelXRule(v, svgImage, bounds, scale);
            }
            for (double v = RoundUp(bounds.Bounds.Y, unitsY); v < bounds.Bounds.Bottom; v += unitsY)
            {
                List<PointF> rule = new()
                {
                    new PointF { Y = (float)v, X = bounds.Bounds.X },
                    new PointF { Y = (float)v, X = bounds.Bounds.Right }
                };
                PlotGraph(rule, svgImage, bounds.Bounds, scale, "gray");
                LabelYRule(v, svgImage, bounds, scale);
            }
        }

        private static void LabelXRule(double v, SVGCreator svgImage, BoundsF bounds, SizeF scale)
        {
            PointF txtLoc = TransformPt(new PointF((float)v, 0), bounds.Bounds, scale);
            LabelPoint(svgImage, v, txtLoc);
        }

        private static void LabelYRule(double v, SVGCreator svgImage, BoundsF bounds, SizeF scale)
        {
            PointF txtLoc = TransformPt(new PointF(0, (float)v), bounds.Bounds, scale);
            LabelPoint(svgImage, v, txtLoc);
        }

        private static void LabelRRule(double v, SVGCreator svgImage, BoundsF bounds, float scale)
        {
            PointF txtLoc = TransformPolar(new PointF(0, (float)v), bounds.Bounds, scale);
            LabelPoint(svgImage, v, txtLoc);
        }

        private static void LabelPoint(SVGCreator svgImage, double v, PointF location)
            => LabelPoint(svgImage, v.ToString("G3"), location);

        private static void LabelPoint(SVGCreator svgImage, string label, PointF location)
        {
            int txtHeight = 20;
            location.X += txtHeight / 2;
            location.Y -= txtHeight / 2;
            svgImage.AddText
                (label, location, $"{txtHeight}px", "sans-serif", false, false, "gray");
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

        public static string PlotGraph(IEnumerable<PointF> points, int width, int height, string color = null)
        {
            List<IEnumerable<PointF>> pointLists = new() { points };
            return PlotGraphs(pointLists, width, height, color);
        }

        private static void PlotGraph(List<PointF> points, SVGCreator svgImage,
            RectangleF bounds, SizeF scale, string penColor)
        {
            var transformedPoints =
                from p in points
                select TransformPt(p, bounds, scale);
            var path = svgImage.AddPolyline(transformedPoints, penColor, 2);
            path.Cap = LineCap.Round;
            path.Join = LineJoin.Round;
        }

        private static void PlotPolarGraph(List<PointF> points, SVGCreator svgImage,
            RectangleF bounds, float scale, string penColor)
        {
            var transformedPoints =
                from p in points
                select TransformPolar(p, bounds, scale);
            var path = svgImage.AddPolyline(transformedPoints, penColor, 2);
            path.Cap = LineCap.Round;
            path.Join = LineJoin.Round;
        }

        private static PointF TransformPt(PointF p, RectangleF bounds, SizeF scale) 
            => new((float)(scale.Width * (p.X - bounds.X)),
                (float)(scale.Height * (bounds.Height - p.Y + bounds.Y)));

        private static PointF TransformPolar(PointF p, RectangleF bounds, float scale)
        {
            float radius = scale * (p.Y - bounds.Y);
            float angle = p.X;
            return new PointF((float)(radius * Math.Cos(angle)), (float)(-radius * Math.Sin(angle)));
        }
    }
}
