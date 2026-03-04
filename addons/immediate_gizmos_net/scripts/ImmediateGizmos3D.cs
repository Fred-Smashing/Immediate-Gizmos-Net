using Godot;

[Tool]
public partial class ImmediateGizmos3D : Node
{
    public static void SetColor(Color color) => ImmediateGizmos.DrawColor = color;
    public static void SetTransform(Transform3D transform) => ImmediateGizmos.Draw3DTransform = transform;
    public static void SetRequiredSelection(Node node) => ImmediateGizmos.DrawRequiredSelection = node;
    public static void SetFont(Font font) => ImmediateGizmos.DrawFont = font;
    public static void SetFontSize(int fontSize) => ImmediateGizmos.DrawFontSize = fontSize;
    public static void Reset() => ImmediateGizmos.Reset();

    public static void Line(Vector3 from, Vector3 to, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawLine3D(from, to);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineStrip(Vector3[] points, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        foreach (var p in points) ImmediateGizmos.DrawPoint3D(p);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LinePolygon(Vector3[] points, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        if (points.Length <= 0) return;
        foreach (var p in points) ImmediateGizmos.DrawPoint3D(p);
        ImmediateGizmos.DrawPoint3D(points[0]);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineArc(Vector3 center, Vector3 axis, Vector3 startPoint, float radians, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawArc3D(center, axis, startPoint, radians);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineCircle(Vector3 center, Vector3 axis, float radius, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        var against = axis.IsEqualApprox(Vector3.Up) ? Vector3.Right : Vector3.Up;
        var startDirection = axis.Cross(against).Normalized();
        ImmediateGizmos.DrawArc3D(center, axis, startDirection * radius, Mathf.Tau);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineSphere(Vector3 center, float radius, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        ImmediateGizmos.DrawArc3D(center, Vector3.Right, Vector3.Up * radius, Mathf.Tau);
        ImmediateGizmos.DrawArc3D(center, Vector3.Forward, Vector3.Up * radius, Mathf.Tau * 0.25f);
        ImmediateGizmos.DrawArc3D(center, Vector3.Up, Vector3.Right * radius, Mathf.Tau);
        ImmediateGizmos.DrawArc3D(center, Vector3.Forward, Vector3.Right * radius, Mathf.Tau * 0.75f);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineCapsule(Vector3 center, float radius, float height, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        height -= radius * 2;
        if (height < 0)
        {
            LineSphere(center, radius, color);
            return;
        }

        var topCenter = center + new Vector3(0.0f, height * 0.5f, 0.0f);
        var bottomCenter = center - new Vector3(0.0f, height * 0.5f, 0.0f);
        var north = Vector3.Forward * radius;
        var east = Vector3.Right * radius;
        var south = Vector3.Back * radius;
        var west = Vector3.Left * radius;

        ImmediateGizmos.DrawArc3D(topCenter, Vector3.Right, north, Mathf.Pi);
        ImmediateGizmos.DrawArc3D(bottomCenter, Vector3.Right, south, Mathf.Pi);
        ImmediateGizmos.DrawArc3D(topCenter, Vector3.Up, north, Mathf.Tau * 0.25f);
        ImmediateGizmos.DrawArc3D(topCenter, Vector3.Forward, west, Mathf.Pi);
        ImmediateGizmos.DrawArc3D(bottomCenter, Vector3.Forward, east, Mathf.Pi);
        ImmediateGizmos.DrawArc3D(bottomCenter, Vector3.Up, west, Mathf.Tau);
        ImmediateGizmos.DrawArc3D(topCenter, Vector3.Up, west, Mathf.Tau * 0.75f);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineCuboid(Vector3 center, Vector3 radius, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        var tlb = center + (new Vector3(1, 1, -1) * radius);
        var tlf = center + (new Vector3(1, 1, 1) * radius);
        var trb = center + (new Vector3(-1, 1, -1) * radius);
        var trf = center + (new Vector3(-1, 1, 1) * radius);
        var blb = center + (new Vector3(1, -1, -1) * radius);
        var blf = center + (new Vector3(1, -1, 1) * radius);
        var brb = center + (new Vector3(-1, -1, -1) * radius);
        var brf = center + (new Vector3(-1, -1, 1) * radius);

        ImmediateGizmos.DrawLine3D(tlb, tlf);
        ImmediateGizmos.DrawLine3D(tlf, trf);
        ImmediateGizmos.DrawLine3D(trf, trb);
        ImmediateGizmos.DrawLine3D(trb, tlb);

        ImmediateGizmos.DrawLine3D(blb, blf);
        ImmediateGizmos.DrawLine3D(blf, brf);
        ImmediateGizmos.DrawLine3D(brf, brb);
        ImmediateGizmos.DrawLine3D(brb, blb);

        ImmediateGizmos.DrawLine3D(tlb, blb);
        ImmediateGizmos.DrawLine3D(tlf, blf);
        ImmediateGizmos.DrawLine3D(trb, brb);
        ImmediateGizmos.DrawLine3D(trf, brf);
        ImmediateGizmos.EndDraw3D(color);
    }

    public static void LineCube(Vector3 center, float radius, Color color = default)
    {
        if (color == default) color = ImmediateGizmos.GizmoDefaultColor;
        LineCuboid(center, Vector3.One * radius, color);
    }

    public static void DrawText(string text, Vector3 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Bottom, float height = 0.25f)
    {
        ImmediateGizmos.DrawText3DInternal(text, position, hAlign, vAlign, height);
    }
}
