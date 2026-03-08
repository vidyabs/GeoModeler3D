using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Rendering;

public interface IEntityRenderer
{
    Type SupportedEntityType { get; }
    Visual3D CreateVisual(IGeometricEntity entity);
    void UpdateVisual(IGeometricEntity entity, Visual3D visual);
    void DisposeVisual(Visual3D visual);
}
