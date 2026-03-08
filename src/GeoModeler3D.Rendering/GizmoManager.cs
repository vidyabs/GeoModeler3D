using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

/// <summary>
/// Manages interactive translate/rotate/scale gizmos in the viewport.
/// Stub — to be implemented in a future phase.
/// </summary>
public class GizmoManager
{
    private HelixViewport3D? _viewport;

    public void Initialize(HelixViewport3D viewport)
    {
        _viewport = viewport;
    }

    public void ShowTranslateGizmo(Guid entityId) { }
    public void ShowRotateGizmo(Guid entityId) { }
    public void ShowScaleGizmo(Guid entityId) { }
    public void HideGizmo() { }
}
