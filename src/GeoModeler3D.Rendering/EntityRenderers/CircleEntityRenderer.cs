using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class CircleEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(CircleEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var circle = (CircleEntity)entity;
        var visual = new LinesVisual3D { Thickness = 2 };
        BuildLines(circle, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        BuildLines((CircleEntity)entity, (LinesVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void BuildLines(CircleEntity circle, LinesVisual3D visual)
    {
        visual.Points.Clear();
        visual.Color = circle.Color.ToWpfColor();

        var normal = Vector3.Normalize(circle.Normal);
        var tangent = GetPerpendicular(normal);
        var bitangent = Vector3.Cross(normal, tangent);

        var segments = circle.SegmentCount;
        for (int i = 0; i < segments; i++)
        {
            var angle1 = 2.0 * System.Math.PI * i / segments;
            var angle2 = 2.0 * System.Math.PI * (i + 1) / segments;

            var p1 = circle.Center
                + (float)(circle.Radius * System.Math.Cos(angle1)) * tangent
                + (float)(circle.Radius * System.Math.Sin(angle1)) * bitangent;

            var p2 = circle.Center
                + (float)(circle.Radius * System.Math.Cos(angle2)) * tangent
                + (float)(circle.Radius * System.Math.Sin(angle2)) * bitangent;

            visual.Points.Add(p1.ToPoint3D());
            visual.Points.Add(p2.ToPoint3D());
        }
    }

    private static Vector3 GetPerpendicular(Vector3 normal)
    {
        var candidate = System.Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) < 0.9f
            ? Vector3.UnitX
            : Vector3.UnitY;
        return Vector3.Normalize(Vector3.Cross(normal, candidate));
    }
}
