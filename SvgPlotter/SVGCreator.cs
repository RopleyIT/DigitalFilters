using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SvgPlotter;

// TODO: Modify to take multiple paths in format that can be fed to Cut2D

/*
 <?xml version="1.0" encoding="UTF-8" standalone="no"?>
    <svg width="391" height="391" viewBox="-70.5 -70.5 391 391" xmlns="http://www.w3.org/2000/svg">
    <rect fill="#fff" stroke="#000" x="-70" y="-70" width="390" height="390"/>
    <g opacity="0.8">
        <rect x="25" y="25" width="200" height="200" fill="green" stroke-width="4" stroke="pink" />
        <circle cx="125" cy="125" r="75" fill="orange" />
        <polyline points="50,150 50,200 200,200 200,100" stroke="red" stroke-width="4" fill="none" />
        <line x1="50" y1="50" x2="200" y2="200" stroke="blue" stroke-width="4" />
    </g>
    </svg>
 */
public class SVGCreator
{
    private readonly List<IRenderable> svgElements;

    public SVGCreator() => svgElements = new List<IRenderable>();

    public bool InvertYAxis { get; set; }

    private IRenderable AddElement(IRenderable element)
    {
        svgElements.Add(element);
        return element;
    }

    private IRenderable AddWithStyle(IRenderable r, string stroke, double strokeWidth, string fill)
    {
        r.Stroke = stroke;
        r.StrokeWidth = strokeWidth.ToString();
        r.Fill = fill;
        return AddElement(r);
    }

    public IRenderable AddPath(IEnumerable<PointF> points, bool close, string stroke, double strokeWidth, string fill)
        => AddWithStyle(new SVGPath(points, close), stroke, strokeWidth, fill);

    public IRenderable AddEllipse(PointF centre, SizeF radii, string stroke, double strokeWidth, string fill)
    {
        SVGPath path = new();
        path.MoveTo(new PointF(centre.X - radii.Width, centre.Y));
        path.Arc(radii.Width, radii.Height, 0, false, false, centre.X + radii.Width, centre.Y);
        path.Arc(radii.Width, radii.Height, 0, false, false, centre.X - radii.Width, centre.Y);
        path.Close();
        return AddWithStyle(path, stroke, strokeWidth, fill);
    }

    public IRenderable AddText(string text, PointF location, string fontSize = "20px",
        string fontName = "sans-serif", bool italic = false, bool bold = false, string fill = "")
    {
        IRenderable r = new SvgText(text, location, fontSize, fontName, italic, bold);
        if (!string.IsNullOrWhiteSpace(fill))
            r.Fill = fill;
        return AddElement(r);
    }

    public IRenderable AddCircle(PointF centre, float radius, string stroke, double strokeWidth, string fill)
        => AddEllipse(centre, new SizeF(radius, radius), stroke, strokeWidth, fill);

    public IRenderable AddRoundedRect(RectangleF r, SizeF rnd, string stroke, double strokeWidth, string fill)
    {
        SVGPath path = new();
        path.MoveTo(new PointF(r.Left, r.Top + rnd.Height));
        path.Arc(rnd.Width, rnd.Height, 0, false, true, r.Left + rnd.Width, r.Top);
        path.LineTo(r.Right - rnd.Width, r.Top);
        path.Arc(rnd.Width, rnd.Height, 0, false, true, r.Right, r.Top + rnd.Height);
        path.LineTo(new PointF(r.Right, r.Bottom - rnd.Height));
        path.Arc(rnd.Width, rnd.Height, 0, false, true, r.Right - rnd.Width, r.Bottom);
        path.LineTo(new PointF(r.Left + rnd.Width, r.Bottom));
        path.Arc(rnd.Width, rnd.Height, 0, false, true, r.Left, r.Bottom - rnd.Height);
        path.Close();
        return AddWithStyle(path, stroke, strokeWidth, fill);
    }

    public IRenderable AddRect(RectangleF r, string stroke, double strokeWidth, string fill)
    {
        var corners = new PointF[]
        {
                new(r.Left, r.Top),
                new(r.Right, r.Top),
                new(r.Right, r.Bottom),
                new(r.Left, r.Bottom)
        };
        return AddPath(corners, true, stroke, strokeWidth, fill);
    }

    public IRenderable AddLine(PointF start, PointF end, string stroke, double strokeWidth)
        => AddPath(new PointF[] { start, end }, false, stroke, strokeWidth, null);

    public IRenderable AddPolyline(IEnumerable<PointF> points, string stroke, double strokeWidth)
        => AddPath(points, false, stroke, strokeWidth, null);

    public IRenderable AddPolygon(IEnumerable<PointF> points, string stroke, double strokeWidth, string fill)
        => AddPath(points, true, stroke, strokeWidth, fill);


    public SizeF DocumentDimensions { get; set; }

    public string DocumentDimensionUnits { get; set; }

    public RectangleF ViewBoxDimensions { get; set; }

    public string ViewBoxDimensionUnits { get; set; }

    public string InfoComment { get; set; }

    private static string XmlHeader => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>";

    private string StartSvg =>
        $"<svg width=\"{DocumentDimensions.Width}{DocumentDimensionUnits}\" "
        + $"height=\"{DocumentDimensions.Height}{DocumentDimensionUnits}\" "
        + $"viewBox=\"{ViewBoxDimensions.X}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Y}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Width}{ViewBoxDimensionUnits} "
        + $"{ViewBoxDimensions.Height}{ViewBoxDimensionUnits}\" "
        + $"xmlns=\"http://www.w3.org/2000/svg\">";

    private static string EndSvg => "</svg>";

    public void CalculateViewBox(SizeF margin)
    {
        if (svgElements != null && svgElements.Count > 0)
        {
            ViewBoxDimensions = svgElements[0].BoundingBox();
            foreach (IRenderable p in svgElements.Skip(1))
                ViewBoxDimensions = SVGPath.Union(ViewBoxDimensions, p.BoundingBox());
        }
        RectangleF vBox = ViewBoxDimensions;
        vBox.Inflate(margin);
        ViewBoxDimensions = vBox;
        DocumentDimensions = new SizeF(ViewBoxDimensions.Width, ViewBoxDimensions.Height);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.WriteLine(XmlHeader);
        if (!string.IsNullOrWhiteSpace(InfoComment))
        {
            sw.WriteLine("<!--");
            sw.Write(InfoComment);
            sw.WriteLine("-->");
        }
        sw.WriteLine(StartSvg);
        if (InvertYAxis)
            sw.WriteLine($"<g transform=\"matrix(1 0 0 -1 0 {DocumentDimensions.Height}\"");
        foreach (IRenderable element in svgElements)
            sw.WriteLine(element.ToString());
        if (InvertYAxis)
            sw.WriteLine("</g>");
        sw.WriteLine(EndSvg);
        return sw.ToString();
    }


    public static RectangleF CreateNormalisedRect(PointF a, PointF b)
        => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
            Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
}
