using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class SphereEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(SphereEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var sphere = (SphereEntity)entity;
        var visual = new SphereVisual3D();
        Apply(sphere, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((SphereEntity)entity, (SphereVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(SphereEntity sphere, SphereVisual3D visual)
    {
        visual.Center = sphere.Center.ToPoint3D();
        visual.Radius = sphere.Radius;
        visual.ThetaDiv = 32;
        visual.PhiDiv = 32;
        visual.Material = MaterialHelper.CreateMaterial(sphere.Color.ToWpfColor());
        visual.Visible = sphere.IsVisible;
    }
}
