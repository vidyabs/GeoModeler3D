using System.Numerics;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using GeoModeler3D.Rendering;

namespace GeoModeler3D.App.Services;

/// <summary>
/// Wires the <see cref="CuttingPlaneManipulator"/> gizmo into the application lifecycle:
///
/// <list type="bullet">
///   <item>Attaches the gizmo when a <see cref="CuttingPlaneEntity"/> is selected.</item>
///   <item>Detaches it when selection changes to anything else, or when the active
///     cutting plane is removed from the scene.</item>
///   <item>On drag completion, pushes a single undoable <see cref="MacroCommand"/>
///     so the whole drag gesture appears as one Ctrl+Z step.</item>
///   <item>Re-positions the gizmo when the plane is moved by external means
///     (undo/redo, properties panel) while the gizmo is attached.</item>
/// </list>
/// </summary>
public class ManipulatorService
{
    private readonly SelectionManager _selectionManager;
    private readonly SceneManager _sceneManager;
    private readonly UndoManager _undoManager;
    private readonly CuttingPlaneManipulator _manipulator;

    public ManipulatorService(
        SelectionManager selectionManager,
        SceneManager sceneManager,
        UndoManager undoManager,
        CuttingPlaneManipulator manipulator)
    {
        _selectionManager = selectionManager;
        _sceneManager     = sceneManager;
        _undoManager      = undoManager;
        _manipulator      = manipulator;

        _selectionManager.SelectionChanged += OnSelectionChanged;
        _sceneManager.EntityRemoved        += OnEntityRemoved;
        _sceneManager.EntityChanged        += OnEntityChanged;
        _manipulator.DragCompleted         += OnDragCompleted;
    }

    // ── scene / selection event handlers ─────────────────────────────────────

    private void OnSelectionChanged()
    {
        var id = _selectionManager.SelectedIds.Count > 0
            ? _selectionManager.SelectedIds[0]
            : (Guid?)null;

        var plane = id.HasValue
            ? _sceneManager.GetById(id.Value) as CuttingPlaneEntity
            : null;

        if (plane is not null)
            _manipulator.AttachTo(plane);
        else
            _manipulator.Detach();
    }

    private void OnEntityRemoved(Guid id)
    {
        if (_manipulator.ActivePlaneId == id)
            _manipulator.Detach();
    }

    private void OnEntityChanged(IGeometricEntity entity)
    {
        // Reposition the gizmo when the plane moves externally (undo, properties panel)
        // without disturbing an in-progress drag.
        if (entity is CuttingPlaneEntity cp && cp.Id == _manipulator.ActivePlaneId)
            _manipulator.UpdatePosition();
    }

    // ── drag completion → single undo entry ──────────────────────────────────

    private void OnDragCompleted(
        CuttingPlaneEntity plane,
        Vector3 preDragOrigin,
        Vector3 preDragNormal)
    {
        var postDragOrigin = plane.Origin;
        var postDragNormal = plane.Normal;

        // Guard: if nothing actually moved, don't pollute the undo stack.
        if (postDragOrigin == preDragOrigin && postDragNormal == preDragNormal)
            return;

        // Build a macro so Ctrl+Z undoes both Origin and Normal in one step.
        // Execute() sets the properties to their current (post-drag) values — a
        // benign no-op visually, but required by UndoManager's Execute-then-push contract.
        var cmdOrigin = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Origin), preDragOrigin, postDragOrigin);

        var cmdNormal = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Normal), preDragNormal, postDragNormal);

        _undoManager.Execute(new MacroCommand("Move Cutting Plane", [cmdOrigin, cmdNormal]));
    }
}
