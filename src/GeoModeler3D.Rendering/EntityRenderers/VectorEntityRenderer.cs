using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class VectorEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(VectorEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var arrow = new ArrowVisual3D();
        Apply((VectorEntity)entity, arrow);
        return arrow;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        Apply((VectorEntity)entity, (ArrowVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void Apply(VectorEntity vector, ArrowVisual3D arrow)
    {
        arrow.Point1 = vector.Origin.ToPoint3D();
        arrow.Point2 = vector.Tip.ToPoint3D();
        arrow.Diameter = 0.08;
        arrow.Material = MaterialHelper.CreateMaterial(vector.Color.ToWpfColor());
        arrow.Visible = vector.IsVisible;
    }
}
