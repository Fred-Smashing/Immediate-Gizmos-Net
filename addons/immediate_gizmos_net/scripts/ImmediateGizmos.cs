using Godot;
using System.Collections.Generic;

[Tool]
public partial class ImmediateGizmos : Node
{
    private const int TargetProcessPriority = -999;
    private const int ExpectedId = 0x1eaf1e55;
    private int _id = ExpectedId;

    public static ImmediateGizmos GizmoRoot { get; private set; }
    public static ShaderMaterial GizmoMaterialLine2D = ResourceLoader.Load<ShaderMaterial>("res://addons/immediate_gizmos_net/materials/immediate_gizmos_line_2d.tres");
    public static ShaderMaterial GizmoMaterialLine3D = ResourceLoader.Load<ShaderMaterial>("res://addons/immediate_gizmos_net/materials/immediate_gizmos_line_3d.tres");
    public static ShaderMaterial GizmoMaterialText2D = ResourceLoader.Load<ShaderMaterial>("res://addons/immediate_gizmos_net/materials/immediate_gizmos_text_2d.tres");
    public static ShaderMaterial GizmoMaterialText3D = ResourceLoader.Load<ShaderMaterial>("res://addons/immediate_gizmos_net/materials/immediate_gizmos_text_3d.tres");

    public static readonly Color GizmoDefaultColor = new Color(0, 0, 0, 0); // Key
    public static readonly Vector2I GizmoTextViewportSize = new Vector2I(4096, 4096); // Key

    public static Color DrawColor = Colors.White;
    public static Transform2D Draw2DTransform = Transform2D.Identity;
    public static Transform3D Draw3DTransform = Transform3D.Identity;
    public static Font DrawFont = ThemeDB.FallbackFont;
    public static int DrawFontSize = 20;
    public static int DrawFontMaxWidth = 512;
    public static Node DrawRequiredSelection = null;

    public static bool IsRequiredSelectionMet()
    {
        if (DrawRequiredSelection == null) return true;
        if (!Engine.IsEditorHint()) return false;

        var sceneRoot = GetSceneRoot();
        var selected = DrawRequiredSelection;

        var editorInterface = Engine.GetSingleton("EditorInterface") as EditorInterface;
        while (editorInterface != null && selected != sceneRoot && selected != null)
        {
            if (editorInterface.GetSelection().GetSelectedNodes().Contains(selected))
                return true;
            selected = selected.GetParent();
        }
        return false;
    }

    public static Color GetColor(Color color)
    {
        if (color == GizmoDefaultColor)
            return DrawColor;
        return color;
    }

    public static void Reset()
    {
        DrawColor = Colors.White;
        Draw2DTransform = Transform2D.Identity;
        Draw3DTransform = Transform3D.Identity;
        DrawFont = ThemeDB.FallbackFont;
        DrawFontSize = 20;
        DrawFontMaxWidth = 512;
        DrawRequiredSelection = null;
    }

    public abstract class RenderBlock
    {
        public List<Node> MeshInstances = new List<Node>();
        public List<ImmediateMesh> Meshes = new List<ImmediateMesh>();
        public int InstanceCounter = 0;
        public bool Is3D;
        public ShaderMaterial Material;
        public bool DuplicateMaterial;

        protected RenderBlock(bool is3D, ShaderMaterial material, bool duplicateMaterial)
        {
            Is3D = is3D;
            Material = material;
            DuplicateMaterial = duplicateMaterial;
        }

        protected void AddInstance()
        {
            Node meshInstance = null;
            ImmediateMesh mesh = new ImmediateMesh();
            ShaderMaterial mat = DuplicateMaterial ? (ShaderMaterial)Material.Duplicate() : Material;

            if (Is3D)
            {
                var mesh3D = new MeshInstance3D();
                mesh3D.Mesh = mesh;
                mesh3D.MaterialOverride = mat;
                meshInstance = mesh3D;
            }
            else
            {
                var mesh2D = new MeshInstance2D();
                mesh2D.Mesh = mesh;
                mesh2D.TopLevel = true;
                mesh2D.Material = mat;
                meshInstance = mesh2D;
            }

            Meshes.Add(mesh);
            MeshInstances.Add(meshInstance);
            GetRoot().AddChild(meshInstance);
        }

        protected bool IsUsableMesh(int index)
        {
            return Meshes[index].GetSurfaceCount() < (int)RenderingServer.MaxMeshSurfaces;
        }

        protected ImmediateMesh GetUsableMesh()
        {
            int index = InstanceCounter;
            while (index < Meshes.Count)
            {
                if (IsUsableMesh(index))
                    return Meshes[index];
                index++;
            }
            AddInstance();
            InstanceCounter = Meshes.Count - 1;
            return Meshes[InstanceCounter];
        }

        public virtual void Clear()
        {
            foreach (var mesh in Meshes)
                mesh.ClearSurfaces();
            InstanceCounter = 0;
        }
    }

    public class LineRenderBlock : RenderBlock
    {
        public LineRenderBlock(bool is3D) : base(is3D, is3D ? GizmoMaterialLine3D : GizmoMaterialLine2D, false) { }

        public void DrawLine2D(Vector2[] points, Color color)
        {
            var mesh = GetUsableMesh();
            mesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
            mesh.SurfaceSetColor(GetColor(color));
            foreach (var point in points)
                mesh.SurfaceAddVertex2D(Draw2DTransform * point);
            mesh.SurfaceEnd();
        }

        public void DrawLine3D(Vector3[] points, Color color)
        {
            var mesh = GetUsableMesh();
            mesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
            mesh.SurfaceSetColor(GetColor(color));
            foreach (var point in points)
                mesh.SurfaceAddVertex(Draw3DTransform * point);
            mesh.SurfaceEnd();
        }
    }

    public partial class TextRenderBlock : RenderBlock
    {
        public List<TextAtlas> TextAtlases = new List<TextAtlas>();

        public TextRenderBlock(bool is3D) : base(is3D, is3D ? GizmoMaterialText3D : GizmoMaterialText2D, true) { }

        private void AddTextAtlas()
        {
            var subViewport = new SubViewport();
            subViewport.Size = GizmoTextViewportSize;
            subViewport.TransparentBg = true;
            var textAtlas = new TextAtlas(subViewport);
            TextAtlases.Add(textAtlas);
            subViewport.AddChild(textAtlas);
            GetRoot().AddChild(subViewport);
        }

        public AtlasDrawInfo DrawTextAtlas(string text, Font font, int fontSize)
        {
            foreach (var textAtlas in TextAtlases)
            {
                var rect = textAtlas.DrawTextAtlas(text, font, fontSize);
                if (rect.Position.X < 0 || rect.Position.Y < 0)
                    continue; // Skip and try next or new
                if (rect.HasArea())
                    return new AtlasDrawInfo(textAtlas.GetUV(rect), textAtlas);
            }
            AddTextAtlas();
            var newTextAtlas = TextAtlases[TextAtlases.Count - 1];
            var newRect = newTextAtlas.DrawTextAtlas(text, font, fontSize);
            if (newRect.HasArea())
                return new AtlasDrawInfo(newTextAtlas.GetUV(newRect), newTextAtlas);
            return null;
        }

        public class AtlasDrawInfo
        {
            public Rect2 UVRect;
            public TextAtlas Atlas;
            public AtlasDrawInfo(Rect2 uvRect, TextAtlas atlas)
            {
                UVRect = uvRect;
                Atlas = atlas;
            }
        }

        private ImmediateMesh GetAtlasMesh(TextAtlas atlas)
        {
            foreach (int meshIndex in atlas.Meshes)
            {
                if (Meshes[meshIndex].GetSurfaceCount() < (int)RenderingServer.MaxMeshSurfaces)
                    return Meshes[meshIndex];
            }
            AddInstance();
            int newIndex = MeshInstances.Count - 1;
            var material = Is3D ? (ShaderMaterial)((MeshInstance3D)MeshInstances[newIndex]).MaterialOverride : (ShaderMaterial)((MeshInstance2D)MeshInstances[newIndex]).Material;
            material.SetShaderParameter("textAtlas", atlas.Viewport.GetTexture());
            atlas.Meshes.Add(newIndex);
            return Meshes[newIndex];
        }

        public void DrawText2D(string text, Vector2 position, HorizontalAlignment hAlign, VerticalAlignment vAlign, float height)
        {
            var drawInfo = DrawTextAtlas(text, DrawFont, DrawFontSize);
            if (drawInfo == null) return;
            var mesh = GetAtlasMesh(drawInfo.Atlas);
            if (mesh == null) return;

            var uv_tl = drawInfo.UVRect.Position;
            var uv_br = uv_tl + drawInfo.UVRect.Size;
            var uv_bl = new Vector2(uv_tl.X, uv_br.Y);
            var uv_tr = new Vector2(uv_br.X, uv_tl.Y);

            var v_bl = new Vector2(0.0f, 0.0f);
            var v_tr = new Vector2(drawInfo.UVRect.Size.X / drawInfo.UVRect.Size.Y, -1.0f) * height;
            var offset = position;
            switch (hAlign)
            {
                case HorizontalAlignment.Center: offset.X -= v_tr.X * 0.5f; break;
                case HorizontalAlignment.Right: offset.X -= v_tr.X; break;
            }
            switch (vAlign)
            {
                case VerticalAlignment.Center: offset.Y -= v_tr.Y * 0.5f; break;
                case VerticalAlignment.Top: offset.Y -= v_tr.Y; break;
            }
            v_bl += offset;
            v_tr += offset;
            var v_tl = new Vector2(v_bl.X, v_tr.Y);
            var v_br = new Vector2(v_tr.X, v_bl.Y);

            v_bl = Draw2DTransform * v_bl;
            v_tr = Draw2DTransform * v_tr;
            v_tl = Draw2DTransform * v_tl;
            v_br = Draw2DTransform * v_br;

            mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
            mesh.SurfaceSetColor(DrawColor);
            
            mesh.SurfaceSetUV(uv_bl);
            mesh.SurfaceAddVertex2D(v_bl);
            mesh.SurfaceSetUV(uv_tl);
            mesh.SurfaceAddVertex2D(v_tl);
            mesh.SurfaceSetUV(uv_br);
            mesh.SurfaceAddVertex2D(v_br);

            mesh.SurfaceSetUV(uv_tl);
            mesh.SurfaceAddVertex2D(v_tl);
            mesh.SurfaceSetUV(uv_tr);
            mesh.SurfaceAddVertex2D(v_tr);
            mesh.SurfaceSetUV(uv_br);
            mesh.SurfaceAddVertex2D(v_br);
            
            mesh.SurfaceEnd();
        }

        public void DrawText3D(string text, Vector3 position, HorizontalAlignment hAlign, VerticalAlignment vAlign, float height)
        {
            var drawInfo = DrawTextAtlas(text, DrawFont, DrawFontSize);
            if (drawInfo == null) return;
            var mesh = GetAtlasMesh(drawInfo.Atlas);
            if (mesh == null) return;

            var uv_tl = drawInfo.UVRect.Position;
            var uv_br = uv_tl + drawInfo.UVRect.Size;
            var uv_bl = new Vector2(uv_tl.X, uv_br.Y);
            var uv_tr = new Vector2(uv_br.X, uv_tl.Y);

            var v_bl = new Vector3(0.0f, 0.0f, 0.0f);
            var v_tr = new Vector3(drawInfo.UVRect.Size.X / drawInfo.UVRect.Size.Y, 1.0f, 0.0f) * height;
            var offset = position;
            switch (hAlign)
            {
                case HorizontalAlignment.Center: offset.X -= v_tr.X * 0.5f; break;
                case HorizontalAlignment.Right: offset.X -= v_tr.X; break;
            }
            switch (vAlign)
            {
                case VerticalAlignment.Center: offset.Y -= v_tr.Y * 0.5f; break;
                case VerticalAlignment.Top: offset.Y -= v_tr.Y; break;
            }
            v_bl += offset;
            v_tr += offset;
            var v_tl = new Vector3(v_bl.X, v_tr.Y, position.Z);
            var v_br = new Vector3(v_tr.X, v_bl.Y, position.Z);

            v_bl = Draw3DTransform * v_bl;
            v_tr = Draw3DTransform * v_tr;
            v_tl = Draw3DTransform * v_tl;
            v_br = Draw3DTransform * v_br;

            mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
            mesh.SurfaceSetColor(DrawColor);
            
            mesh.SurfaceSetUV(uv_bl);
            mesh.SurfaceAddVertex(v_bl);
            mesh.SurfaceSetUV(uv_tl);
            mesh.SurfaceAddVertex(v_tl);
            mesh.SurfaceSetUV(uv_br);
            mesh.SurfaceAddVertex(v_br);

            mesh.SurfaceSetUV(uv_tl);
            mesh.SurfaceAddVertex(v_tl);
            mesh.SurfaceSetUV(uv_tr);
            mesh.SurfaceAddVertex(v_tr);
            mesh.SurfaceSetUV(uv_br);
            mesh.SurfaceAddVertex(v_br);

            // Backside? Original code has 12 vertices for 3D text (front and back)
            mesh.SurfaceSetUV(uv_br);
            mesh.SurfaceAddVertex(v_bl);
            mesh.SurfaceSetUV(uv_bl);
            mesh.SurfaceAddVertex(v_br);
            mesh.SurfaceSetUV(uv_tr);
            mesh.SurfaceAddVertex(v_tl);

            mesh.SurfaceSetUV(uv_tr);
            mesh.SurfaceAddVertex(v_tl);
            mesh.SurfaceSetUV(uv_bl);
            mesh.SurfaceAddVertex(v_br);
            mesh.SurfaceSetUV(uv_tl);
            mesh.SurfaceAddVertex(v_tr);

            mesh.SurfaceEnd();
        }

        public override void Clear()
        {
            base.Clear();
            foreach (var atlas in TextAtlases)
                atlas.Clear();
        }

        public partial class TextAtlas : Node2D
        {
            public Viewport Viewport;
            public Rect2I Region;
            public List<int> Meshes = new List<int>();
            public List<BinStrings> BinStringsList = new List<BinStrings>();
            public List<int> BinXs = new List<int> { 0 };
            public List<int> BinYs = new List<int> { 0 };

            public class BinStrings
            {
                public Rect2I Rect;
                public string Text;
                public Font Font;
                public int FontSize;
                public int MaxWidth;
                public BinStrings(Rect2I rect, string text, Font font, int fontSize, int maxWidth)
                {
                    Rect = rect;
                    Text = text;
                    Font = font;
                    FontSize = fontSize;
                    MaxWidth = maxWidth;
                }
            }

            public TextAtlas(SubViewport viewport)
            {
                Viewport = viewport;
                Region = new Rect2I(Vector2I.Zero, viewport.Size);
            }

            public Rect2I DrawTextAtlas(string text, Font font, int fontSize)
            {
                int maxWidth = Mathf.Min(Region.Size.X, DrawFontMaxWidth);
                Vector2I sizei = (Vector2I)font.GetMultilineStringSize(text, HorizontalAlignment.Left, maxWidth, fontSize).Ceil();
                
                int xIndex = 0;
                int yIndex = 0;
                Rect2I rect = new Rect2I(Vector2I.Zero, sizei);
                if (!Region.Encloses(rect))
                    return new Rect2I(-Vector2I.One, Vector2I.Zero);

                bool overlapping = true;
                while (overlapping && yIndex < BinYs.Count)
                {
                    overlapping = false;
                    foreach (var bin in BinStringsList)
                    {
                        if (rect.Intersects(bin.Rect))
                        {
                            overlapping = true;
                            break;
                        }
                    }
                    if (!overlapping) break;

                    xIndex++;
                    if (xIndex < BinXs.Count)
                    {
                        rect.Position = new Vector2I(BinXs[xIndex], rect.Position.Y);
                        if ((rect.Position.X + rect.Size.X) <= (Region.Position.X + Region.Size.X))
                            continue;
                    }
                    xIndex = 0;
                    yIndex++;
                    if (yIndex < BinYs.Count)
                    {
                        rect.Position = new Vector2I(BinXs[xIndex], BinYs[yIndex]);
                    }
                }

                if (!Region.Encloses(rect))
                    return new Rect2I();

                int nextX = rect.Position.X + rect.Size.X;
                if (!BinXs.Contains(nextX))
                {
                    int idx = BinXs.BinarySearch(nextX);
                    if (idx < 0) BinXs.Insert(~idx, nextX);
                }

                int nextY = rect.Position.Y + rect.Size.Y;
                if (!BinYs.Contains(nextY))
                {
                    int idx = BinYs.BinarySearch(nextY);
                    if (idx < 0) BinYs.Insert(~idx, nextY);
                }

                BinStringsList.Add(new BinStrings(rect, text, font, fontSize, maxWidth));
                QueueRedraw();
                return rect;
            }

            public override void _Draw()
            {
                foreach (var binString in BinStringsList)
                {
                    float ascent = binString.Font.GetAscent(binString.FontSize);
                    DrawMultilineString(binString.Font, new Vector2(binString.Rect.Position.X, binString.Rect.Position.Y) + (Vector2.Down * ascent), binString.Text, HorizontalAlignment.Left, binString.MaxWidth, binString.FontSize);
                }
            }

            public Rect2 GetUV(Rect2I rect)
            {
                return new Rect2((Vector2)rect.Position / (Vector2)Region.Size, (Vector2)rect.Size / (Vector2)Region.Size);
            }

            public void Clear()
            {
                BinXs = new List<int> { 0 };
                BinYs = new List<int> { 0 };
                BinStringsList.Clear();
            }
        }
    }

    public enum RenderMode
    {
        GizmosLine2D,
        GizmosLine3D,
        GizmosText2D,
        GizmosText3D,
    }

    public class RenderSelector
    {
        public RenderBlock[] ProcessBlock = new RenderBlock[2];
        public RenderMode Mode;

        public RenderSelector(RenderMode mode)
        {
            Mode = mode;
        }

        private RenderBlock CreateProcessBlock()
        {
            switch (Mode)
            {
                case RenderMode.GizmosLine2D: return new LineRenderBlock(false);
                case RenderMode.GizmosLine3D: return new LineRenderBlock(true);
                case RenderMode.GizmosText2D: return new TextRenderBlock(false);
                case RenderMode.GizmosText3D: return new TextRenderBlock(true);
                default: GD.PushError("Unsupported ImmediateGizmos RenderMode"); return null;
            }
        }

        private int GetBlockIndex()
        {
            return Engine.IsInPhysicsFrame() ? 1 : 0;
        }

        public RenderBlock GetRenderBlock()
        {
            int index = GetBlockIndex();
            if (ProcessBlock[index] == null)
                ProcessBlock[index] = CreateProcessBlock();
            return ProcessBlock[index];
        }

        public void Clear()
        {
            int index = GetBlockIndex();
            if (ProcessBlock[index] != null)
                ProcessBlock[index].Clear();
        }
    }

    private static Dictionary<RenderMode, RenderSelector> _renderSelectors = new Dictionary<RenderMode, RenderSelector>();

    public static RenderSelector GetRenderSelector(RenderMode renderMode)
    {
        if (_renderSelectors.TryGetValue(renderMode, out var selector))
            return selector;
        selector = new RenderSelector(renderMode);
        _renderSelectors[renderMode] = selector;
        return selector;
    }

    public static RenderBlock GetRenderBlock(RenderMode renderMode)
    {
        return GetRenderSelector(renderMode).GetRenderBlock();
    }

    public static Node GetSceneRoot()
    {
        if (Engine.IsEditorHint())
        {
            var editorInterface = Engine.GetSingleton("EditorInterface") as EditorInterface;
            return editorInterface.GetEditedSceneRoot()?.GetParent();
        }

        if (ProjectSettings.GetSetting("application/run/main_loop_type").AsString() == "SceneTree")
        {
            return ((SceneTree)Engine.GetMainLoop()).Root;
        }
        return null;
    }

    public static ImmediateGizmos GetRoot()
    {
        if (GizmoRoot != null) return GizmoRoot;

        var sceneRoot = GetSceneRoot();
        if (sceneRoot == null) return null;

        int rootCounter = 1;
        string rootName = "ImmediateGizmos";
        Node rootNode = sceneRoot.FindChild(rootName, false, false);
        while (rootNode != null)
        {
            if (rootNode is ImmediateGizmos gizmos && gizmos._id == ExpectedId)
            {
                GizmoRoot = gizmos;
                return GizmoRoot;
            }
            rootCounter++;
            rootName = $"ImmediateGizmos{rootCounter}";
            rootNode = sceneRoot.FindChild(rootName, false, false);
        }

        GizmoRoot = new ImmediateGizmos();
        GizmoRoot.Name = rootName;
        sceneRoot.AddChild(GizmoRoot);
        return GizmoRoot;
    }

    private static List<Vector2> _points2D = new List<Vector2>();
    public static void DrawPoint2D(Vector2 point) => _points2D.Add(point);

    public static void DrawLine2D(Vector2 from, Vector2 to)
    {
        DrawPoint2D(from);
        DrawPoint2D(to);
    }

    public static void DrawArc2D(Vector2 center, Vector2 startPoint, float radians)
    {
        radians = Mathf.Clamp(radians, 0.0f, Mathf.Tau);
        if (radians <= 0.0f) return;

        int detail = (int)Mathf.Ceil((radians / Mathf.Tau) * 32.0f);
        float increment = radians / detail;

        for (int i = 0; i <= detail; i++)
        {
            var pos = startPoint.Rotated(i * increment);
            DrawPoint2D(center + pos);
        }
    }

    public static void EndDraw2D(Color color)
    {
        if (_points2D.Count > 0 && IsRequiredSelectionMet())
        {
            var renderBlock = GetRenderBlock(RenderMode.GizmosLine2D) as LineRenderBlock;
            if (renderBlock != null)
                renderBlock.DrawLine2D(_points2D.ToArray(), color);
        }
        _points2D.Clear();
    }

    private static List<Vector3> _points3D = new List<Vector3>();
    public static void DrawPoint3D(Vector3 point) => _points3D.Add(point);

    public static void DrawLine3D(Vector3 from, Vector3 to)
    {
        DrawPoint3D(from);
        DrawPoint3D(to);
    }

    public static void DrawArc3D(Vector3 center, Vector3 axis, Vector3 startPoint, float radians)
    {
        radians = Mathf.Clamp(radians, 0.0f, Mathf.Tau);
        if (radians <= 0.0f) return;

        int detail = (int)Mathf.Ceil((radians / Mathf.Tau) * 32.0f);
        float increment = radians / detail;

        for (int i = 0; i <= detail; i++)
        {
            var pos = startPoint.Rotated(axis, i * increment);
            DrawPoint3D(center + pos);
        }
    }

    public static void EndDraw3D(Color color)
    {
        if (_points3D.Count > 0 && IsRequiredSelectionMet())
        {
            var renderBlock = GetRenderBlock(RenderMode.GizmosLine3D) as LineRenderBlock;
            if (renderBlock != null)
                renderBlock.DrawLine3D(_points3D.ToArray(), color);
        }
        _points3D.Clear();
    }

    public static void DrawText2DInternal(string text, Vector2 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Bottom, float height = 0.25f)
    {
        var renderBlock = GetRenderBlock(RenderMode.GizmosText2D) as TextRenderBlock;
        if (IsRequiredSelectionMet() && renderBlock != null)
            renderBlock.DrawText2D(text, position, hAlign, vAlign, height);
    }

    public static void DrawText3DInternal(string text, Vector3 position, HorizontalAlignment hAlign = HorizontalAlignment.Left, VerticalAlignment vAlign = VerticalAlignment.Bottom, float height = 0.25f)
    {
        var renderBlock = GetRenderBlock(RenderMode.GizmosText3D) as TextRenderBlock;
        if (IsRequiredSelectionMet() && renderBlock != null)
            renderBlock.DrawText3D(text, position, hAlign, vAlign, height);
    }

    public override void _Ready()
    {
        ProcessPriority = TargetProcessPriority;
        ProcessPhysicsPriority = TargetProcessPriority;
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public override void _Process(double delta)
    {
        foreach (var selector in _renderSelectors.Values)
            selector.Clear();
        Reset();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var selector in _renderSelectors.Values)
            selector.Clear();
        Reset();
    }
}
