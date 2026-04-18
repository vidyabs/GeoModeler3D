using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class CuttingPlaneEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(CuttingPlaneEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var container = new ModelVisual3D();
        Apply((CuttingPlaneEntity)entity, container);
        return container;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((CuttingPlaneEntity)entity, (ModelVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(CuttingPlaneEntity plane, ModelVisual3D visual)
    {
        if (!plane.IsVisible)
        {
            visual.Content = null;
            visual.Children.Clear();
            return;
        }

        float hw = (float)(plane.DisplayWidth / 2);
        float hh = (float)(plane.DisplayHeight / 2);
        var (u, v) = PlaneEntity.ComputeTangents(Vector3.Normalize(plane.Normal));

        var p0 = plane.Origin - hw * u - hh * v;
        var p1 = plane.Origin + hw * u - hh * v;
        var p2 = plane.Origin + hw * u + hh * v;
        var p3 = plane.Origin - hw * u + hh * v;

        // Semi-transparent quad (double-sided)
        var geo = new MeshGeometry3D();
        geo.Positions.Add(p0.ToPoint3D());
        geo.Positions.Add(p1.ToPoint3D());
        geo.Positions.Add(p2.ToPoint3D());
        geo.Positions.Add(p3.ToPoint3D());
        geo.TriangleIndices.Add(0); geo.TriangleIndices.Add(1); geo.TriangleIndices.Add(2);
        geo.TriangleIndices.Add(0); geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(3);
        geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(1); geo.TriangleIndices.Add(0);
        geo.TriangleIndices.Add(3); geo.TriangleIndices.Add(2); geo.TriangleIndices.Add(0);

        var wpfColor = plane.Color.ToWpfColor();
        var opacity = System.Math.Clamp(plane.Opacity, 0.0, 1.0);
        var mat = MaterialHelper.CreateMaterial(wpfColor, opacity);
        visual.Content = new GeometryModel3D(geo, mat) { BackMaterial = mat };

        // Border outline
        visual.Children.Clear();
        var border = new LinesVisual3D
        {
            Color = Colors.CornflowerBlue,
            Thickness = 1.5
        };
        border.Points.Add(p0.ToPoint3D()); border.Points.Add(p1.ToPoint3D());
        border.Points.Add(p1.ToPoint3D()); border.Points.Add(p2.ToPoint3D());
        border.Points.Add(p2.ToPoint3D()); border.Points.Add(p3.ToPoint3D());
        border.Points.Add(p3.ToPoint3D()); border.Points.Add(p0.ToPoint3D());
        visual.Children.Add(border);

        // Normal indicator arrow
        var tip = plane.Origin + Vector3.Normalize(plane.Normal) * (float)(System.Math.Min(hw, hh) * 0.4);
        var arrow = new ArrowVisual3D
        {
            Point1 = plane.Origin.ToPoint3D(),
            Point2 = tip.ToPoint3D(),
            Diameter = System.Math.Min(hw, hh) * 0.04,
            Fill = new SolidColorBrush(Colors.CornflowerBlue)
        };
        visual.Children.Add(arrow);
    }
}
