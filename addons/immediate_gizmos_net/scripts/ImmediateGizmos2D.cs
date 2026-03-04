using Godot;

[Tool]
public partial class ImmediateGizmos2D : Node
{
    public static void SetColor(Color color) => ImmediateGizmos.DrawColor = color;
    public static void SetTransform(Transform2D transform) => ImmediateGizmos.Draw2DTransform = transform;
    public static void SetRequiredSelection(Node node) => ImmediateGizmos.DrawRequiredSelection = node;
    public static void SetFont(Font font) => ImmediateGizmos.DrawFont = font;
    public static void SetFontSize(int fontSize) => ImmediateGizmos.DrawFontSize = fontSize;
    public static void Reset() => ImmediateGizmos.Reset();

    public static void Line(Vector2 from, Vector2 to, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawLine2D(from, to);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineStrip(Vector2[] points, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        foreach (var p in points) ImmediateGizmos.DrawPoint2D(p);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LinePolygon(Vector2[] points, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        if (points.Length <= 0) return;
        foreach (var p in points) ImmediateGizmos.DrawPoint2D(p);
        ImmediateGizmos.DrawPoint2D(points[0]);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineArc(Vector2 center, Vector2 startPoint, float radians, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawArc2D(center, startPoint, radians);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineCircle(Vector2 center, float radius, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawArc2D(center, Vector2.Up * radius, Mathf.Tau);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineCapsule(Vector2 center, float radius, float height, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        height -= radius * 2;
        if (height < 0)
        {
            LineCircle(center, radius, color);
            return;
        }

        var topCenter = center + new Vector2(0.0f, height * 0.5f);
        var bottomCenter = center - new Vector2(0.0f, height * 0.5f);
        var east = Vector2.Right * radius;
        var west = Vector2.Left * radius;

        ImmediateGizmos.DrawArc2D(topCenter, east, Mathf.Pi);
        ImmediateGizmos.DrawArc2D(bottomCenter, west, Mathf.Pi);
        ImmediateGizmos.DrawPoint2D(topCenter + east);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineRect(Vector2 center, Vector2 size, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        var tl = center + (new Vector2(-1, -1) * size);
        var tr = center + (new Vector2(1, -1) * size);
        var bl = center + (new Vector2(-1, 1) * size);
        var br = center + (new Vector2(1, 1) * size);

        ImmediateGizmos.DrawLine2D(tl, tr);
        ImmediateGizmos.DrawLine2D(tr, br);
        ImmediateGizmos.DrawLine2D(br, bl);
        ImmediateGizmos.DrawLine2D(bl, tl);
        ImmediateGizmos.EndDraw2D(color);
    }

    public static void LineSquare(Vector2 center, float size, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        LineRect(center, Vector2.One * size, color);
    }

    public static void DrawText(string text, Vector2 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Bottom, float height = 0.25f)
    {
        ImmediateGizmos.DrawText2DInternal(text, position, hAlign, vAlign, height);
    }
}
