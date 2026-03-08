using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.EntityRenderers;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

public class RenderingService : IRenderingService
{
    private readonly EntityRendererRegistry _rendererRegistry;
    private readonly SelectionHighlighter _highlighter;
    private HelixViewport3D? _viewport;

    private readonly Dictionary<Guid, Visual3D> _entityVisuals = new();
    private readonly Dictionary<Visual3D, Guid> _visualToEntity = new();

    public RenderingService(EntityRendererRegistry rendererRegistry, SelectionHighlighter highlighter)
    {
        _rendererRegistry = rendererRegistry;
        _highlighter = highlighter;
    }

    public void Initialize(HelixViewport3D viewport)
    {
        _viewport = viewport;
    }

    public void AddEntity(IGeometricEntity entity)
    {
        if (_viewport is null || _entityVisuals.ContainsKey(entity.Id)) return;
        if (!_rendererRegistry.HasRenderer(entity.GetType())) return;

        var renderer = _rendererRegistry.GetRenderer(entity.GetType());
        var visual = renderer.CreateVisual(entity);

        _entityVisuals[entity.Id] = visual;
        _visualToEntity[visual] = entity.Id;
        _viewport.Children.Add(visual);
    }

    public void UpdateEntity(IGeometricEntity entity)
    {
        if (!_entityVisuals.TryGetValue(entity.Id, out var visual)) return;
        if (!_rendererRegistry.HasRenderer(entity.GetType())) return;

        var renderer = _rendererRegistry.GetRenderer(entity.GetType());
        renderer.UpdateVisual(entity, visual);
    }

    public void RemoveEntity(Guid entityId)
    {
        if (!_entityVisuals.TryGetValue(entityId, out var visual)) return;

        _highlighter.RemoveHighlight(entityId, visual);

        if (_rendererRegistry.HasRenderer(visual.GetType()))
        {
            // best-effort dispose
        }

        _viewport?.Children.Remove(visual);
        _entityVisuals.Remove(entityId);
        _visualToEntity.Remove(visual);
    }

    public void RefreshDirtyEntities()
    {
        // Currently updates are pushed via UpdateEntity calls.
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        // TODO: implement wireframe/shaded/shaded-with-edges switching
    }

    public void HighlightEntities(IEnumerable<Guid> entityIds)
    {
        _highlighter.ClearAll(_entityVisuals);
        foreach (var id in entityIds)
        {
            if (_entityVisuals.TryGetValue(id, out var visual))
                _highlighter.Highlight(id, visual);
        }
    }

    public void ClearHighlight()
    {
        _highlighter.ClearAll(_entityVisuals);
    }

    public Guid? GetEntityIdFromVisual(Visual3D visual)
    {
        // Walk up the visual tree to find a registered visual
        Visual3D? current = visual;
        while (current != null)
        {
            if (_visualToEntity.TryGetValue(current, out var id))
                return id;
            current = VisualTreeHelper.GetParent(current) as Visual3D;
        }
        return null;
    }

    public RenderTargetBitmap CaptureFrame(int width, int height)
    {
        var rtb = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        if (_viewport != null)
            rtb.Render(_viewport);
        return rtb;
    }
}
