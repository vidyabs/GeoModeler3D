using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class CylinderEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(CylinderEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var cylinder = (CylinderEntity)entity;
        var visual = new PipeVisual3D();
        Apply(cylinder, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((CylinderEntity)entity, (PipeVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(CylinderEntity cylinder, PipeVisual3D visual)
    {
        visual.Point1 = cylinder.BaseCenter.ToPoint3D();
        visual.Point2 = cylinder.TopCenter.ToPoint3D();
        visual.Diameter = cylinder.Radius * 2;
        visual.InnerDiameter = 0; // solid cylinder, not hollow pipe
        visual.ThetaDiv = 32;
        visual.Material = MaterialHelper.CreateMaterial(cylinder.Color.ToWpfColor());
        visual.Visible = cylinder.IsVisible;
    }
}
