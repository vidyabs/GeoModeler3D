using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class ContourCurveEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(ContourCurveEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var container = new ModelVisual3D();
        Apply((ContourCurveEntity)entity, container);
        return container;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((ContourCurveEntity)entity, (ModelVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(ContourCurveEntity contour, ModelVisual3D visual)
    {
        visual.Children.Clear();

        if (!contour.IsVisible || contour.Points.Count < 2) return;

        var lines = new LinesVisual3D
        {
            Color = contour.Color.ToWpfColor(),
            Thickness = 2.0
        };

        var pts = contour.Points;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            lines.Points.Add(pts[i].ToPoint3D());
            lines.Points.Add(pts[i + 1].ToPoint3D());
        }

        if (contour.IsClosed && pts.Count >= 3)
        {
            lines.Points.Add(pts[pts.Count - 1].ToPoint3D());
            lines.Points.Add(pts[0].ToPoint3D());
        }

        visual.Children.Add(lines);
    }
}
