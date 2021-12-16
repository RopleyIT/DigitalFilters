using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace SvgPlotter;

public class SvgText : IRenderable
{
    public string Stroke { get; set; }
    public string StrokeWidth { get; set; }
    public string Fill { get; set; }
    public LineCap Cap { get; set; } = LineCap.None;
    public LineJoin Join { get; set; } = LineJoin.None;
    public IEnumerable<int> Dashes { get; set; }

    public void SetDashes(params int[] dashes)
        => Dashes = dashes;

    public string FontSize { get; set; } = "20px";

    // serif; sans-serif; monospace; or cursive
    public string FontName { get; set; } = "sans-serif";

    public bool Italic { get; set; } = false;

    public bool Bold { get; set; } = false;

    private readonly string text;
    private PointF location;
    public SvgText(string text, PointF btmLeft, string size, string font, bool italic, bool bold)
    {
        this.text = text;
        location = btmLeft;
        FontSize = size;
        FontName = font;
        Italic = italic;
        Bold = bold;
    }

    public RectangleF BoundingBox()
    {
        // TODO: Incorporate this into drawing size
        return RectangleF.Empty;
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.Write($"<text x=\"{location.X}\" y=\"{location.Y}\" style=\"font:");
        if (Italic)
            sw.Write(" italic");
        if (Bold)
            sw.Write(" bold");
        sw.Write($" {FontSize} {FontName};\"");
        if (!string.IsNullOrEmpty(Fill))
            sw.Write($" fill=\"{Fill}\"");
        sw.Write($">{WebUtility.HtmlEncode(text)}</text>");
        return sw.ToString();
    }
}
