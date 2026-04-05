using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class PlaneEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(PlaneEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var visual = new ModelVisual3D();
        Apply((PlaneEntity)entity, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((PlaneEntity)entity, (ModelVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(PlaneEntity plane, ModelVisual3D visual)
    {
        if (!plane.IsVisible)
        {
            visual.Content = null;
            return;
        }

        float h = (float)(plane.DisplaySize / 2);
        var (u, v) = PlaneEntity.ComputeTangents(Vector3.Normalize(plane.Normal));

        var p0 = plane.Origin - h * u - h * v;
        var p1 = plane.Origin + h * u - h * v;
        var p2 = plane.Origin + h * u + h * v;
        var p3 = plane.Origin - h * u + h * v;

        var geo = new MeshGeometry3D();
        geo.Positions.Add(p0.ToPoint3D());
        geo.Positions.Add(p1.ToPoint3D());
        geo.Positions.Add(p2.ToPoint3D());
        geo.Positions.Add(p3.ToPoint3D());

        // Front face
        geo.TriangleIndices.Add(0); geo.TriangleIndices.Add(1); geo.TriangleIndices.Add(2);
        geo.TriangleIndices.Add(0); geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(3);
        // Back face
        geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(1); geo.TriangleIndices.Add(0);
        geo.TriangleIndices.Add(3); geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(0);

        var wpfColor = plane.Color.ToWpfColor();
        wpfColor.A = 160;
        var mat = MaterialHelper.CreateMaterial(wpfColor, wpfColor.A / 255.0);
        visual.Content = new GeometryModel3D(geo, mat) { BackMaterial = mat };
    }
}
