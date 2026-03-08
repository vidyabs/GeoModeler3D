using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class ConeEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(ConeEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var cone = (ConeEntity)entity;
        var visual = new TruncatedConeVisual3D();
        Apply(cone, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((ConeEntity)entity, (TruncatedConeVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(ConeEntity cone, TruncatedConeVisual3D visual)
    {
        visual.Origin = cone.BaseCenter.ToPoint3D();
        visual.Normal = cone.Axis.ToVector3D();
        visual.BaseRadius = cone.BaseRadius;
        visual.TopRadius = 0; // pointed cone
        visual.Height = cone.Height;
        visual.TopCap = true;
        visual.BaseCap = true;
        visual.ThetaDiv = 32;
        visual.Material = MaterialHelper.CreateMaterial(cone.Color.ToWpfColor());
        visual.Visible = cone.IsVisible;
    }
}
