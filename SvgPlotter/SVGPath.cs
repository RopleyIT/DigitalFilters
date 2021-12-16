using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SvgPlotter;

public enum StrokeType
{
    Move,
    Line,
    Cubic,
    Quadratic,
    Arc,
    Z
};

public class SVGPath : IRenderable
{
    // Factory methods for common shapes

    /// <summary>
    /// Create a closed or open path using a sequence of points
    /// </summary>
    /// <param name="points">The sequence of points to be joined
    /// using straight line segments</param>
    /// <param name="closed">True to close the 
    /// last point back to the first with a line</param>
    /// <returns>The path created</returns>

    public static SVGPath FromPoints(IEnumerable<PointF> points, bool closed)
        => new(points, closed);

    public string Stroke { get; set; }
    public string StrokeWidth { get; set; }
    public string Fill { get; set; }
    public LineCap Cap { get; set; } = LineCap.None;
    public LineJoin Join { get; set; } = LineJoin.None;
    public IEnumerable<int> Dashes { get; set; }

    public void SetDashes(params int[] dashes)
        => Dashes = dashes;

    public IList<SVGPathElement> Elements { get; private set; }
        = new List<SVGPathElement>();

    public SVGPath(IEnumerable<PointF> points, bool closed)
    {
        if (points == null || !points.Any())
            throw new ArgumentException("No points for SVG path");

        PointF prev = points.First();
        Elements.Add(SVGPathElement.MoveTo(prev));
        foreach (PointF p in points.Skip(1))
        {
            if (p != prev)
                Elements.Add(SVGPathElement.LineTo(p));
            prev = p;
        }
        if (closed)
            Elements.Add(SVGPathElement.Close());
    }

    public SVGPath() { }

    public void MoveTo(PointF p)
        => Elements.Add(SVGPathElement.MoveTo(p.X, p.Y));

    public void MoveRel(float dx, float dy)
        => Elements.Add(SVGPathElement.MoveRel(dx, dy));

    public void LineTo(PointF p)
        => Elements.Add(SVGPathElement.LineTo(p));

    public void LineTo(float x, float y)
        => Elements.Add(SVGPathElement.LineTo(x, y));

    public void LineRel(float dx, float dy)
        => Elements.Add(SVGPathElement.LineRel(dx, dy));

    public void Close()
        => Elements.Add(SVGPathElement.Close());

    public void Cubic(float cx1, float cy1, float cx2, float cy2, float x, float y)
        => Elements.Add(SVGPathElement.Cubic(cx1, cy1, cx2, cy2, x, y));

    public void CubicRel(float dx1, float dy1, float dx2, float dy2, float dx, float dy)
        => Elements.Add(SVGPathElement.CubicRel(dx1, dy1, dx2, dy2, dx, dy));

    public void Quadratic(float cx1, float cy1, float x, float y)
        => Elements.Add(SVGPathElement.Quadratic(cx1, cy1, x, y));

    public void QuadraticRel(float dx1, float dy1, float dx, float dy)
        => Elements.Add(SVGPathElement.QuadraticRel(dx1, dy1, dx, dy));

    public void Arc(float rx, float ry, float angle, bool largeArc, bool sweep, float x, float y)
        => Elements.Add(SVGPathElement.Arc(rx, ry, angle, largeArc, sweep, x, y));

    public void ArcRel(float rx, float ry, float angle, bool largeArc, bool sweep, float dx, float dy)
        => Elements.Add(SVGPathElement.ArcRel(rx, ry, angle, largeArc, sweep, dx, dy));


    public RectangleF BoundingBox()
    {
        RectangleF bbox = RectangleF.Empty;
        var elementsWithPoints =
            from e in Elements
            where e.Points != null && e.Points.Count > 0
            select e;

        if (elementsWithPoints.Any())
        {
            bbox = elementsWithPoints.First().BoundingBox();
            foreach (SVGPathElement e in elementsWithPoints.Skip(1))
                bbox = SVGPath.Union(bbox, e.BoundingBox());
        }
        return bbox;
    }

    public static RectangleF Union(RectangleF r, RectangleF s)
    {
        float xMax = Math.Max(r.Right, s.Right);
        float xMin = Math.Min(r.Left, s.Left);
        float yMax = Math.Max(r.Bottom, s.Bottom);
        float yMin = Math.Min(r.Top, s.Top);
        return new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.Write("<path d=\"");
        int i = 0;
        foreach (SVGPathElement pe in Elements)
        {
            sw.Write(pe.ToString());
            if (++i % 10 == 0)
                sw.WriteLine();
        }
        sw.Write("\"");
        sw.Write(RenderStye());
        sw.WriteLine("/>");
        return sw.ToString();
    }

    public string RenderStye()
    {
        StringWriter sw = new();
        if (!string.IsNullOrEmpty(Stroke))
            sw.Write($" stroke=\"{Stroke}\"");
        if (!string.IsNullOrEmpty(Fill))
            sw.Write($" fill=\"{Fill}\"");
        else
            sw.Write($" fill=\"transparent\"");
        if (!string.IsNullOrEmpty(StrokeWidth))
            sw.Write($" stroke-width=\"{StrokeWidth}\"");
        var strCap = Cap switch
        {
            LineCap.Round => "round",
            LineCap.Square => "square",
            LineCap.Butt => "butt",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strCap))
            sw.Write($" stroke-linecap=\"{strCap}\"");
        var strJoin = Join switch
        {
            LineJoin.Round => "round",
            LineJoin.Mitre => "mitre",
            LineJoin.Bevel => "bevel",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strJoin))
            sw.Write($" stroke-linejoin=\"{strJoin}\"");
        if (Dashes != null && Dashes.Any())
        {
            sw.Write($" stroke-dasharray=\"{Dashes.First()}");
            foreach (int i in Dashes.Skip(1))
                sw.Write($",{i}");
            sw.Write("\"");
        }
        return sw.ToString();
    }

}

public class SVGPathElement
{
    public StrokeType StrokeType { get; private set; }
    public bool Relative { get; private set; }

    private readonly PointF[] points;

    public IList<PointF> Points => points;

    private SVGPathElement(StrokeType type, bool rel)
    {
        StrokeType = type;
        Relative = rel;
        if (type == StrokeType.Cubic)
            points = new PointF[3];
        else if (type == StrokeType.Quadratic)
            points = new PointF[2];
        else if (type == StrokeType.Arc)
            points = new PointF[3];
        else if (type == StrokeType.Z)
            points = Array.Empty<PointF>();
        else
            points = new PointF[1];
    }

    public static SVGPathElement MoveTo(PointF p)
    {
        SVGPathElement element = new(StrokeType.Move, false);
        element.points[0] = p;
        return element;
    }

    public static SVGPathElement MoveTo(float x, float y)
        => MoveTo(new PointF(x, y));

    public static SVGPathElement MoveRel(float dx, float dy)
    {
        SVGPathElement element = new(StrokeType.Move, true);
        element.points[0] = new PointF(dx, dy);
        return element;
    }

    public static SVGPathElement LineTo(PointF p)
    {
        SVGPathElement element = new(StrokeType.Line, false);
        element.Points[0] = p;
        return element;
    }

    public static SVGPathElement LineTo(float x, float y)
        => LineTo(new PointF(x, y));

    public static SVGPathElement LineRel(float dx, float dy)
    {
        SVGPathElement element = new(StrokeType.Line, true);
        element.points[0] = new PointF(dx, dy);
        return element;
    }

    public static SVGPathElement Close() => new(StrokeType.Z, false);

    public static SVGPathElement Cubic(float cx1, float cy1, float cx2, float cy2, float x, float y)
    {
        SVGPathElement element = new(StrokeType.Cubic, false);
        element.points[0] = new PointF(cx1, cy1);
        element.points[1] = new PointF(cx2, cy2);
        element.points[2] = new PointF(x, y);
        return element;
    }

    public static SVGPathElement CubicRel(float dx1, float dy1, float dx2, float dy2, float dx, float dy)
    {
        SVGPathElement element = new(StrokeType.Cubic, true);
        element.points[0] = new PointF(dx1, dy1);
        element.points[1] = new PointF(dx2, dy2);
        element.points[2] = new PointF(dx, dy);
        return element;
    }

    public static SVGPathElement Quadratic(float cx1, float cy1, float x, float y)
    {
        SVGPathElement element = new(StrokeType.Cubic, false);
        element.points[0] = new PointF(cx1, cy1);
        element.points[1] = new PointF(x, y);
        return element;
    }

    public static SVGPathElement QuadraticRel(float dx1, float dy1, float dx, float dy)
    {
        SVGPathElement element = new(StrokeType.Cubic, true);
        element.points[0] = new PointF(dx1, dy1);
        element.points[1] = new PointF(dx, dy);
        return element;
    }

    public static SVGPathElement Arc(float rx, float ry, float angle, bool largeArc, bool sweep, float x, float y)
    {
        SVGPathElement element = new(StrokeType.Arc, false);
        element.points[0] = new PointF(rx, ry);
        element.points[1] = new PointF(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
        element.points[2] = new PointF(x, y);
        return element;
    }

    public static SVGPathElement ArcRel(float rx, float ry, float angle, bool largeArc, bool sweep, float dx, float dy)
    {
        SVGPathElement element = new(StrokeType.Arc, true);
        element.points[0] = new PointF(rx, ry);
        element.points[1] = new PointF(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
        element.points[2] = new PointF(dx, dy);
        return element;
    }

    private const string TypeStrings = "MLCQAZ";
    private const string RelTypeStrings = "mlcqaz";
    private static readonly string[] ArcStrings = { "0,0", "1,0", "0,1", "1,1" };

    public override string ToString()
    {
        string typeString = Relative ? RelTypeStrings : TypeStrings;
        string result = typeString[(int)StrokeType].ToString();
        switch (StrokeType)
        {
            case StrokeType.Move:
            case StrokeType.Line:
                result += RenderPoint(points[0]);
                break;
            case StrokeType.Quadratic:
                result += RenderPoints(points, 2);
                break;
            case StrokeType.Cubic:
                result += RenderPoints(points, 3);
                break;
            case StrokeType.Arc:
                result += RenderPoint(points[0]);
                result += $",{points[1].X:F2},{ArcStrings[(int)points[1].Y]},";
                result += RenderPoint(points[2]);
                break;
        }
        return result;
    }

    public RectangleF BoundingBox()
    {
        // TODO: Fix this for all shapes
        float xMax = points.Select(p => p.X).Max();
        float xMin = points.Select(p => p.X).Min();
        float yMax = points.Select(p => p.Y).Max();
        float yMin = points.Select(p => p.Y).Min();
        return new(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    /// <summary>
    /// Calculate the width and height of one quarter of the bounding box
    /// surrounding a rotated ellipse
    /// </summary>
    /// <param name="rx">X axis radius of unrotated ellipse</param>
    /// <param name="ry">Y axis radius of unrotated ellipse</param>
    /// <param name="angle">The anticlockwise angle through which
    /// the ellipse has been rotated (radians)</param>
    /// <returns>The width and height of one quarter of the bounding box
    /// surrounding the rotated ellipse</returns>

    private static SizeF EllipseQuarterBounds(float rx, float ry, float angle)
    {
        var cosAngle = Math.Cos(angle);
        var sinAngle = Math.Sin(angle);
        var xr = Math.Sqrt(Sqr(rx * cosAngle) + Sqr(ry * sinAngle));
        var yr = Math.Sqrt(Sqr(rx * sinAngle) + Sqr(ry * cosAngle));
        return new SizeF((float)xr, (float)yr);
    }

    private static double Sqr(double v) => v * v;

    private static string RenderPoint(PointF p) => $"{p.X:F2},{p.Y:F2}";

    private static string RenderPoints(IEnumerable<PointF> pe, int count)
        => string.Join(",", pe.Take(count).Select(p => RenderPoint(p)));
}
