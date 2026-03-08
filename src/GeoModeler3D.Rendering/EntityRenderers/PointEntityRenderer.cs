using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class PointEntityRenderer : IEntityRenderer
{
    private const double PointDisplayRadius = 0.15;

    public Type SupportedEntityType => typeof(PointEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var point = (PointEntity)entity;
        var visual = new SphereVisual3D();
        Apply(point, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((PointEntity)entity, (SphereVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(PointEntity point, SphereVisual3D visual)
    {
        visual.Center = point.Position.ToPoint3D();
        visual.Radius = PointDisplayRadius;
        visual.ThetaDiv = 12;
        visual.PhiDiv = 12;
        visual.Material = MaterialHelper.CreateMaterial(point.Color.ToWpfColor());
        visual.Visible = point.IsVisible;
    }
}
