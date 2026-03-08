using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

public interface IRenderingService
{
    void Initialize(HelixViewport3D viewport);
    void AddEntity(IGeometricEntity entity);
    void UpdateEntity(IGeometricEntity entity);
    void RemoveEntity(Guid entityId);
    void RefreshDirtyEntities();
    void SetDisplayMode(DisplayMode mode);
    void HighlightEntities(IEnumerable<Guid> entityIds);
    void ClearHighlight();
    Guid? GetEntityIdFromVisual(Visual3D visual);
    RenderTargetBitmap CaptureFrame(int width, int height);
}
