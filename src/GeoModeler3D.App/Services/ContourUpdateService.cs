using System.Windows.Threading;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Core.Services;

namespace GeoModeler3D.App.Services;

/// <summary>
/// Listens to scene changes and maintains computed ContourCurveEntity objects.
/// Contour curves are auto-generated and are NOT part of the undo history.
/// </summary>
public class ContourUpdateService
{
    private readonly SceneManager _sceneManager;
    private readonly ContourExtractionService _extractor;
    private readonly DispatcherTimer _debounceTimer;

    // plane ID → list of generated contour IDs
    private readonly Dictionary<Guid, List<Guid>> _planeContours = new();
    private bool _recomputing;

    public ContourUpdateService(SceneManager sceneManager, ContourExtractionService extractor)
    {
        _sceneManager = sceneManager;
        _extractor = extractor;

        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _debounceTimer.Tick += (_, _) => { _debounceTimer.Stop(); RecomputeAll(); };

        _sceneManager.EntityAdded += OnEntityAdded;
        _sceneManager.EntityChanged += OnEntityChanged;
        _sceneManager.EntityRemoved += OnEntityRemoved;
    }

    private void OnEntityAdded(IGeometricEntity entity)
    {
        if (_recomputing || entity is ContourCurveEntity) return;
        Schedule();
    }

    private void OnEntityChanged(IGeometricEntity entity)
    {
        if (_recomputing || entity is ContourCurveEntity) return;
        Schedule();
    }

    private void OnEntityRemoved(Guid id)
    {
        if (_recomputing) return;

        // Cutting plane removed — clean up its contours immediately
        if (_planeContours.TryGetValue(id, out var contourIds))
        {
            _recomputing = true;
            try
            {
                foreach (var cid in contourIds)
                    _sceneManager.Remove(cid);
            }
            finally
            {
                _recomputing = false;
                _planeContours.Remove(id);
            }
            Schedule(); // recompute remaining planes (targets may have changed)
            return;
        }

        // Ignore removal of our own generated contour curves
        bool isOwnContour = _planeContours.Values.Any(list => list.Contains(id));
        if (isOwnContour) return;

        Schedule();
    }

    private void Schedule()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void RecomputeAll()
    {
        _recomputing = true;
        try
        {
            var planes = _sceneManager.Entities.OfType<CuttingPlaneEntity>().ToList();

            // Remove all previously generated contours
            var oldIds = _planeContours.Values.SelectMany(x => x).ToList();
            foreach (var id in oldIds)
                _sceneManager.Remove(id);
            _planeContours.Clear();

            foreach (var plane in planes)
            {
                var newIds = new List<Guid>();

                foreach (var targetId in plane.TargetEntityIds)
                {
                    var entity = _sceneManager.GetById(targetId);
                    if (entity is null or ContourCurveEntity) continue;

                    var contours = _extractor.Extract(plane, entity);
                    foreach (var c in contours)
                    {
                        _sceneManager.Add(c);
                        newIds.Add(c.Id);
                    }
                }

                if (newIds.Count > 0)
                    _planeContours[plane.Id] = newIds;
            }
        }
        finally
        {
            _recomputing = false;
        }
    }
}
