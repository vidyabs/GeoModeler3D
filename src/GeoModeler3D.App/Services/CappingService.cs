using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering;

namespace GeoModeler3D.App.Services;

/// <summary>
/// Manages filled cap visuals for cutting-plane cross-sections.
/// Whenever a closed <see cref="ContourCurveEntity"/> is added or updated and
/// the source cutting plane has <see cref="CuttingPlaneEntity.IsCappingEnabled"/>,
/// a filled polygon is added directly to the 3-D viewport.
/// </summary>
public class CappingService
{
    private readonly SceneManager _sceneManager;
    private readonly ViewportManager _viewportManager;

    // contourId → the ModelVisual3D cap that was generated for it
    private readonly Dictionary<Guid, ModelVisual3D> _capVisuals = new();

    public CappingService(SceneManager sceneManager, ViewportManager viewportManager)
    {
        _sceneManager = sceneManager;
        _viewportManager = viewportManager;

        _sceneManager.EntityAdded   += OnEntityAdded;
        _sceneManager.EntityChanged += OnEntityChanged;
        _sceneManager.EntityRemoved += OnEntityRemoved;
    }

    // ── scene event handlers ──────────────────────────────────────────────────

    private void OnEntityAdded(IGeometricEntity entity)
    {
        if (entity is ContourCurveEntity contour)
            TryAddCap(contour);
    }

    private void OnEntityChanged(IGeometricEntity entity)
    {
        switch (entity)
        {
            case ContourCurveEntity contour:
                RemoveCap(contour.Id);
                TryAddCap(contour);
                break;

            case CuttingPlaneEntity plane:
                // IsCappingEnabled or ClipSide may have changed — rebuild all caps for this plane
                RefreshCapsForPlane(plane);
                break;
        }
    }

    private void OnEntityRemoved(Guid id)
    {
        RemoveCap(id);

        // If a cutting plane was removed, ContourUpdateService will remove its contours
        // → OnEntityRemoved will fire for each contour → caps cleaned up automatically.
        // Nothing extra needed here.
    }

    // ── cap lifecycle ─────────────────────────────────────────────────────────

    private void TryAddCap(ContourCurveEntity contour)
    {
        if (!ShouldCap(contour, out var plane)) return;

        var cap = CappingVisualGenerator.Generate(
            contour,
            plane!.Normal,
            plane.Origin,
            ToWpfColor(plane.Color));

        if (cap is null) return;

        _capVisuals[contour.Id] = cap;
        _viewportManager.Viewport?.Children.Add(cap);
    }

    private void RemoveCap(Guid contourId)
    {
        if (!_capVisuals.TryGetValue(contourId, out var cap)) return;
        _capVisuals.Remove(contourId);
        _viewportManager.Viewport?.Children.Remove(cap);
    }

    private void RefreshCapsForPlane(CuttingPlaneEntity plane)
    {
        // Find all contour curves that belong to this plane
        var contours = _sceneManager.Entities
            .OfType<ContourCurveEntity>()
            .Where(c => c.SourcePlaneId == plane.Id)
            .ToList();

        foreach (var contour in contours)
        {
            RemoveCap(contour.Id);
            TryAddCap(contour);
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if a cap should be generated for <paramref name="contour"/>.
    /// Sets <paramref name="plane"/> to the source cutting plane when true.
    /// </summary>
    private bool ShouldCap(ContourCurveEntity contour, out CuttingPlaneEntity? plane)
    {
        plane = _sceneManager.GetById(contour.SourcePlaneId) as CuttingPlaneEntity;
        return plane is not null
            && plane.IsCappingEnabled
            && plane.ClipSide != ClipSide.None
            && contour.IsClosed
            && contour.Points.Count >= 3;
    }

    private static System.Windows.Media.Color ToWpfColor(EntityColor c)
        => System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
}
