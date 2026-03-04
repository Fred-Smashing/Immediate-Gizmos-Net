# Immediate Gizmos <img height="40px" src="./media/logo-transparent.png" alt="">
Immediate-mode gizmos for all to use!

## Content
- [Immediate Gizmos ](#immediate-gizmos-)
  - [Content](#content)
  - [About](#about)
	- [Examples](#examples)
  - [Usage](#usage)
	- [Lines](#lines)
	  - [Strips](#strips)
	  - [Color](#color)
	  - [Polygons](#polygons)
	  - [Arcs, Circles, Spheres, \& Capsules](#arcs-circles-spheres--capsules)
	  - [Rects, Squares, Cubes, \& Cuboids](#rects-squares-cubes--cuboids)
	  - [Transforms](#transforms)
	- [Text](#text)
	- [In The Editor!](#in-the-editor)
	- [Cleanup](#cleanup)
  - [Method List](#method-list)
	- [State](#state)
	- [2D](#2d)
	- [3D](#3d)
	- [Optional Notes](#optional-notes)

## About
*ImmediateGizmos* is an immediate-mode gizmos drawing plugin/addon for *Godot 4*!
*ImmediateGizmos* is designed to allow you to draw simple gizmos and text wherever and whenever in the code of your project, including in release builds!

### Examples
<img src="./media/usage/spectrum_shift.gif" alt="Example debug usage in the game Spectrum Shift" width=100%>
<img src="./media/usage/dragon_roii.gif" alt="Example usage in the game Dragon RoII" width=100%>
<img src="./media/usage/deadweight.gif" alt="Example usage in the game DEADWEIGHT" width=100%>
<img src="./media/usage/chicken_roii_1.png" alt="Example 1 debug usage in the game Chicken RoII" width=100%>

## Usage
*ImmediateGizmos* are used with through the `ImmediateGizmos2D` and `ImmediateGizmos3D` singletons.
These singletons feature methods for drawing line-based shapes as well as text.

By design, this addon has a similar interface as *Unity*'s Gizmos but with more flexibility with where the draw calls occur and the ability to show in release builds.

### Lines
For drawing lines, the `line` method can be used in both *2D* and *3D*.
This method has the arguments of the point `from`, the point `to`, as well as an optional `color` argument.

```csharp
private void Lines()
{
    ImmediateGizmos3D.Line(Vector3.Zero, Vector3.Right, Colors.Red);
    ImmediateGizmos3D.Line(Vector3.Zero, Vector3.Up, Colors.Green);
    ImmediateGizmos3D.Line(Vector3.Zero, Vector3.Forward, Colors.Blue);
}
```
<img src="./media/examples/lines.png" alt="" min-width=50%>

All line-based methods include the optional `color` argument at the end of their calls, which can be left empty to default to `Color.WHITE`.

#### Strips
Contiguous line strips can be drawn with `line_strip`. This draws an array of points.

```csharp
private void Strips()
{
    var points = new Vector2[10];
    for (int i = 0; i < 10; i++)
    {
        points[i] = new Vector2(i, Mathf.Sin(time + i));
    }
    ImmediateGizmos2D.LineStrip(points);
}
```
<img src="./media/examples/strips.gif" alt="" min-width=50%>

#### Color
Color can also be set for all subsequence calls with the following:

```csharp
// To set to red for example:
var color = Colors.Red;

ImmediateGizmos2D.SetColor(color);
// or
ImmediateGizmos3D.SetColor(color);
```

#### Polygons
The addition of strips is also accompanied by the method `line_strip`, which draws a closed-loop line strip.

```csharp
ImmediateGizmos2D.LinePolygon(points);
```
<img src="./media/examples/polygon.gif" alt="" min-width=50%>

#### Arcs, Circles, Spheres, & Capsules
For 2D you can draw arcs, circles, and capsule shapes:

```csharp
private void ArcsCirclesCapsules()
{
    const float radius = 0.4f;
    ImmediateGizmos2D.LineArc(Vector2.Left, Vector2.Left * radius, Mathf.Tau * 0.5f, Colors.Red);
    ImmediateGizmos2D.LineCircle(Vector2.Zero, radius, Colors.Green);
    ImmediateGizmos2D.LineCapsule(Vector2.Right, radius, 1.5f, Colors.Blue);
}
```
<img src="./media/examples/arcs_circles_capsules.png" alt="" min-width=50%>

And similarly for 3D:

```csharp
private void ArcsCirclesSpheresCapsules()
{
    const float radius = 0.4f;
    ImmediateGizmos3D.LineArc(Vector3.Left * 1.5f, Vector3.Forward, Vector3.Left * radius, Mathf.Tau * 0.5f, Colors.Red);
    ImmediateGizmos3D.LineCircle(Vector3.Left * 0.5f, Vector3.Forward, radius, Colors.Green);
    ImmediateGizmos3D.LineSphere(Vector3.Right * 0.5f, radius, Colors.Yellow);
    ImmediateGizmos3D.LineCapsule(Vector3.Right * 1.5f, radius, 1.5f, Colors.Blue);
}
```
<img src="./media/examples/arcs_circles_spheres_capsules.png" alt="" min-width=50%>

#### Rects, Squares, Cubes, & Cuboids
For 2D you can draw rect and square shapes:

```csharp
private void RectsSquares()
{
    const float radius = 0.8f;
    ImmediateGizmos2D.LineRect(Vector2.Left, new Vector2(radius, radius * 2.0f), Colors.Magenta);
    ImmediateGizmos2D.LineSquare(Vector2.Right, radius, Colors.Green);
}
```
<img src="./media/examples/rects_squares.png" alt="" min-width=50%>

And similarly for 3D:

```csharp
private void CuboidsCubes()
{
    const float radius = 0.8f;
    ImmediateGizmos3D.LineCuboid(Vector3.Left, new Vector3(radius, radius * 2.0f, radius), Colors.Magenta);
    ImmediateGizmos3D.LineCube(Vector3.Right, radius, Colors.Green);
}
```
<img src="./media/examples/cuboids_cubes.png" alt="" min-width=50%>

#### Transforms
Similar to color, a transform can be set that is used for all subsequent draw calls. This allows you to rotate and reposition draw calls as desired.

For 2D:
```csharp
var transform = new Transform2D();
ImmediateGizmos2D.SetTransform(transform);
```
And for 3D:
```csharp
var transform = new Transform3D();
ImmediateGizmos3D.SetTransform(transform);
```

This transform is applied onto the positions parsed into the draw function itself.

```csharp
private void Transforms()
{
    const float radius = 0.4f;

    ImmediateGizmos3D.SetTransform(new Transform3D(Basis.FromEuler(new Vector3(50f, 20f, -40f) * Mathf.DegToRad)));
    ImmediateGizmos3D.LineSphere(Vector3.Left, radius, Colors.Red);

    ImmediateGizmos3D.SetTransform(new Transform3D(Basis.FromEuler(new Vector3(30f, 20f, 10f) * Mathf.DegToRad)));
    ImmediateGizmos3D.LineCube(Vector3.Right, radius, Colors.Green);

    ImmediateGizmos3D.SetTransform(new Transform3D(Basis.FromEuler(new Vector3(10f, 30f, 30f) * Mathf.DegToRad)));
    ImmediateGizmos3D.LineCapsule(Vector3.Right * 1.5f, radius, 1.5f, Colors.Blue);
}
```
<img src="./media/examples/transforms.png" alt="" min-width=50%>

You can also set the draw transform to a node's transform making all subsequent calls relative to that particular node, including translation, rotation, and scaling:
```csharp
private void TransformSelf()
{
    ImmediateGizmos3D.SetTransform(Transform);
    ImmediateGizmos3D.LineCube(Vector3.Zero, 1.0f, Colors.Cyan);
}
```
<img src="./media/examples/transform_self.gif" alt="" min-width=50%>

### Text
set_font
set_font_size

Text drawing is possible with *ImmediateGizmos* and by default from the bottom left.
```csharp
private void Text()
{
    ImmediateGizmos2D.DrawText("Text!", Vector2.Zero);
    ImmediateGizmos3D.DrawText("Text!", Vector3.Zero);
}
```
<img src="./media/examples/text.png" alt="" min-width=50%>

The third and fourth arguments to the `draw_text` method are `HorizontalAlignment` and `VerticalAlignment` typed values respectively.

The `font` and `font_size` can be set like the following:
```csharp
[Export]
private Font font;

[Export]
private int fontSize = 60;

private void TextFont()
{
    ImmediateGizmos2D.SetFont(font);
    ImmediateGizmos2D.SetFontSize(fontSize);
    ImmediateGizmos2D.DrawText("Text!", Vector2.Zero);
}
```
<img src="./media/examples/text_font.png" alt="" min-width=50%>


### In The Editor!
ImmediateGizmos also works nicely with `@tool` scripts in the editor!

This includes a method for only drawing the target node when it's selected!
The following code will only draw the subsequent gizmos when the node, `self`, is being selected in the editor.
```csharp
ImmediateGizmos2D.SetRequiredSelection(this);
// or
ImmediateGizmos3D.SetRequiredSelection(this);
```
<img src="./media/examples/editor.gif" alt="" min-width=50%>

### Cleanup

To guarantee consistent behavior between state changes, you can call the following function **before any subsequent gizmo calls**:
```csharp
ImmediateGizmos.Reset();
```

## Method List

### State
These functions can be accessed either through `ImmediateGizmos2D` or `ImmediateGizmos3D` singletons.

| Method                 | Arguments                 | Description                                                                                |
| ---------------------- | ------------------------- | ------------------------------------------------------------------------------------------ |
| set_color              | `color` : Color           | Sets the default draw color of drawn gizmos.                                               |
| set_transform          | `transform` : Transform2D | Sets the default transform of drawn gizmos.                                                |
| set_required_selection | `node` : Node             | Sets the node that must be selected in the `Editor` for subsequent gizmo calls to show.    |
| set_font               | `font` : Font             | Sets the default font of draw text.                                                        |
| set_font_size          | `fontSize` : int          | Sets the default font size of draw text.                                                   |
| reset                  |                           | Resets all set states back to default. It's recommend to call this before any gizmo calls. |

### 2D
These functions can be accessed through the `ImmediateGizmos2D` singleton.

| Method       | Arguments                                                                                                                       | Description                                                                                          |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| line         | `from` : Vector2<br>`to` : Vector2<br> `color?` : Color                                                                         | Draws a line from point `from` to point `to`.                                                        |
| line_strip   | `points` : Array[Vector2]<br> `color?` : Color                                                                                  | Draws a continuous line from array points.                                                           |
| line_polygon | `points` : Array[Vector2]<br> `color?` : Color                                                                                  | Draws a continuous closed line loop from the array points.                                           |
| line_arc     | `center` : Vector2<br>`startPoint` : Vector2<br> `radians` : float<br>`color?` : Color                                          | Draws a continuous line from `startPoint` rotated by `radians`.                                      |
| line_circle  | `center` : Vector2<br> `radius` : float<br> `color?` : Color                                                                    | Draws a circle of `radius` around `center`.                                                          |
| line_capsule | `center` : Vector2<br> `radius` : float<br> `height` : float<br>`color?` : Color                                                | Draws an axis-aligned capsule of `radius` and `height` around `center`.                              |
| line_rect    | `center` : Vector2<br> `size` : Vector2<br> `color?` : Color                                                                    | Draws an axis-aligned rectangle of `radius` size around `center`.                                    |
| line_square  | `center` : Vector2<br> `size` : float<br> `color?` : Color                                                                      | Draws an axis-aligned square of `radius` size around `center`.                                       |
| draw_text    | `text` : String<br>`position` : Vector2<br>`hAlign?` : HorizontalAlignment<br>`vAlign?` : VerticalAlignment<br>`scale?` : float | Draws `test` at `position` with world height of `height` and aligned based on `hAlign` and `vAlign`. |

### 3D
These functions can be accessed through the `ImmediateGizmos3D` singleton.

| Method       | Arguments                                                                                                                     | Description                                                                                          |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| line         | `from` : Vector3<br>`to` : Vector3<br>`color` : Color                                                                         | Draws a line from point `from` to point `to`.                                                        |
| line_strip   | `points` : Array[Vector3]<br>`color` : Color                                                                                  | Draws a continuous line from array points.                                                           |
| line_polygon | `points` : Array[Vector3]<br>`color` : Color                                                                                  | Draws a continuous closed line loop from the array points.                                           |
| line_arc     | `center` : Vector3<br>`axis` : Vector3<br>`startPoint` : Vector3<br>`radians` : float<br>`color` : Color                      | Draws a continuous line from `startPoint` rotated around the chosen `axis` by `radians`.             |
| line_circle  | `center` : Vector3<br>`axis` : Vector3<br>`radius` : float<br>`color` : Color                                                 | Draws a circle of `radius` around `center` based on `axis`.                                          |
| line_sphere  | `center` : Vector3<br>`radius` : float<br>`color` : Color                                                                     | Draws an axis-aligned sphere of `radius` around `center`.                                            |
| line_capsule | `center` : Vector3<br>`radius` : float<br>`height` : float<br>`color` : Color                                                 | Draws an axis-aligned capsule of `radius` and `height` around `center` .                             |
| line_cuboid  | `center` : Vector3<br>`radius` : Vector3<br>`color` : Color                                                                   | Draws an axis-aligned cuboid of `radius` size around `center`.                                       |
| line_cube    | `center` : Vector3<br>`radius` : float<br>`color` : Color                                                                     | Draws an axis-aligned cube of `radius` size around `center`.                                         |
| draw_text    | `text` : String<br>`position` : Vector3<br>`hAlign` : HorizontalAlignment<br>`vAlign` : VerticalAlignment<br>`height` : float | Draws `test` at `position` with world height of `height` and aligned based on `hAlign` and `vAlign`. |

### Optional Notes
- `color?` defaults to `Color.white`.
- `hAlign?` defaults to `HORIZONTAL_ALIGNMENT_LEFT`.
- `vAlign?` defaults to `VERTICAL_ALIGNMENT_BOTTOM`.
- `height?` defaults to `0.25` in world units.

<hr>

[Immediate Gizmos](https://github.com/jawdan-dev/Immediate-Gizmos) © 2026 by [Jawdan.dev](https://jawdan.dev/) is licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
