using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class CuttingPlaneEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(CuttingPlaneEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        // Stub: return an empty visual for now
        return new ModelVisual3D();
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual) { }

    public void DisposeVisual(Visual3D visual) { }
}
