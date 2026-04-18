using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering.EntityRenderers;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

/// <summary>
/// Manages CuttingPlaneGroup containers for visual mesh clipping.
/// Called by RenderingService when cutting planes or their targets change.
/// </summary>
public class CuttingPlaneVisualizer
{
    private readonly SceneManager _sceneManager;
    private HelixViewport3D? _viewport;
    private Dictionary<Guid, Visual3D>? _entityVisuals;
    private EntityRendererRegistry? _rendererRegistry;

    // per cutting-plane clipping state
    private readonly Dictionary<Guid, PlaneState> _states = new();

    // reverse map: entityId → planeId currently clipping it (one plane per entity)
    private readonly Dictionary<Guid, Guid> _entityToPlane = new();

    public CuttingPlaneVisualizer(SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public void Initialize(HelixViewport3D viewport,
        Dictionary<Guid, Visual3D> entityVisuals,
        EntityRendererRegistry rendererRegistry)
    {
        _viewport = viewport;
        _entityVisuals = entityVisuals;
        _rendererRegistry = rendererRegistry;
    }

    /// <summary>
    /// Called when a CuttingPlaneEntity is added or updated.
    /// Rebuilds the clip groups for this plane.
    /// </summary>
    public void Sync(CuttingPlaneEntity plane)
    {
        TeardownState(plane.Id);

        bool needsClip = plane.ClipSide != ClipSide.None
                      && plane.IsVisible
                      && plane.TargetEntityIds.Count > 0;

        // Always record intended targets so OnEntityAdded can re-sync later
        foreach (var tid in plane.TargetEntityIds)
            _entityToPlane[tid] = plane.Id;

        if (!needsClip || _viewport is null || _entityVisuals is null || _rendererRegistry is null)
            return;

        var state = BuildState(plane);
        if (state is not null)
            _states[plane.Id] = state;
    }

    /// <summary>
    /// Called by RenderingService when any non-CuttingPlane entity is added,
    /// in case a cutting plane was waiting for this target.
    /// </summary>
    public void OnEntityAdded(Guid entityId)
    {
        if (!_entityToPlane.TryGetValue(entityId, out var planeId)) return;
        if (_states.ContainsKey(planeId)) return; // already active

        var plane = _sceneManager.GetById(planeId) as CuttingPlaneEntity;
        if (plane is not null) Sync(plane);
    }

    /// <summary>Called after RenderingService updates a target entity's primary visual.</summary>
    public void OnEntityVisualUpdated(IGeometricEntity entity)
    {
        foreach (var state in _states.Values)
        {
            if (!state.SecondaryVisuals.TryGetValue(entity.Id, out var secondary)) continue;
            if (_rendererRegistry!.HasRenderer(entity.GetType()))
                _rendererRegistry.GetRenderer(entity.GetType()).UpdateVisual(entity, secondary);
        }
    }

    /// <summary>Called when a target entity is removed from the scene.</summary>
    public void OnEntityRemoved(Guid entityId)
    {
        _entityToPlane.Remove(entityId);

        foreach (var state in _states.Values)
        {
            if (!state.TrackedEntities.ContainsKey(entityId)) continue;

            var group1 = state.Groups.Count > 0 ? state.Groups[0] : null;
            var group2 = state.Groups.Count > 1 ? state.Groups[1] : null;

            if (_entityVisuals!.TryGetValue(entityId, out var primary))
                group1?.Children.Remove(primary);

            if (state.SecondaryVisuals.TryGetValue(entityId, out var secondary))
                group2?.Children.Remove(secondary);

            state.TrackedEntities.Remove(entityId);
            state.SecondaryVisuals.Remove(entityId);
        }
    }

    /// <summary>Called when a CuttingPlaneEntity is removed.</summary>
    public void Remove(Guid planeId)
    {
        // Remove reverse-map entries for this plane
        foreach (var key in _entityToPlane.Where(kv => kv.Value == planeId).Select(kv => kv.Key).ToList())
            _entityToPlane.Remove(key);

        TeardownState(planeId);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private PlaneState? BuildState(CuttingPlaneEntity plane)
    {
        var normal = plane.Normal.ToVector3D();
        var origin = plane.Origin.ToPoint3D();

        var group1 = new CuttingPlaneGroup { IsEnabled = true };
        CuttingPlaneGroup? group2 = null;

        switch (plane.ClipSide)
        {
            case ClipSide.Positive:
                group1.CuttingPlanes.Add(new Plane3D(origin, normal));
                break;
            case ClipSide.Negative:
                group1.CuttingPlanes.Add(new Plane3D(origin, -normal));
                break;
            case ClipSide.BothWithGap:
                float gap = (float)(plane.GapDistance / 2.0);
                var offset = plane.Normal * gap;
                group1.CuttingPlanes.Add(new Plane3D((plane.Origin + offset).ToPoint3D(), normal));
                group2 = new CuttingPlaneGroup { IsEnabled = true };
                group2.CuttingPlanes.Add(new Plane3D((plane.Origin - offset).ToPoint3D(), -normal));
                break;
        }

        var trackedEntities = new Dictionary<Guid, IGeometricEntity>();
        var secondaryVisuals = new Dictionary<Guid, Visual3D>();

        foreach (var targetId in plane.TargetEntityIds)
        {
            // Skip if another cutting plane already owns this entity
            if (trackedEntities.ContainsKey(targetId)) continue;

            var entity = _sceneManager.GetById(targetId);
            if (entity is null) continue;
            if (!_entityVisuals!.TryGetValue(targetId, out var primaryVisual)) continue;

            // Move primary visual from viewport → group1
            _viewport!.Children.Remove(primaryVisual);
            group1.Children.Add(primaryVisual);
            trackedEntities[targetId] = entity;

            // BothWithGap: create a secondary copy in group2
            if (group2 is not null && _rendererRegistry!.HasRenderer(entity.GetType()))
            {
                var renderer = _rendererRegistry.GetRenderer(entity.GetType());
                var secondary = renderer.CreateVisual(entity);
                group2.Children.Add(secondary);
                secondaryVisuals[targetId] = secondary;
            }
        }

        if (trackedEntities.Count == 0) return null;

        _viewport!.Children.Add(group1);
        var groups = new List<CuttingPlaneGroup> { group1 };

        if (group2 is not null)
        {
            _viewport.Children.Add(group2);
            groups.Add(group2);
        }

        return new PlaneState(groups, secondaryVisuals, trackedEntities);
    }

    private void TeardownState(Guid planeId)
    {
        if (!_states.TryGetValue(planeId, out var state)) return;
        _states.Remove(planeId);

        var group1 = state.Groups.Count > 0 ? state.Groups[0] : null;

        // Move primary visuals back to viewport and refresh them
        foreach (var (entityId, entity) in state.TrackedEntities)
        {
            if (!_entityVisuals!.TryGetValue(entityId, out var visual)) continue;

            group1?.Children.Remove(visual);
            _viewport!.Children.Add(visual);

            // Re-render to undo any geometry modification by CuttingPlaneGroup
            if (_rendererRegistry!.HasRenderer(entity.GetType()))
                _rendererRegistry.GetRenderer(entity.GetType()).UpdateVisual(entity, visual);
        }

        // Remove groups from viewport
        foreach (var group in state.Groups)
            _viewport?.Children.Remove(group);
    }

    private record PlaneState(
        List<CuttingPlaneGroup> Groups,
        Dictionary<Guid, Visual3D> SecondaryVisuals,
        Dictionary<Guid, IGeometricEntity> TrackedEntities
    );
}
