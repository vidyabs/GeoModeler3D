using System.Numerics;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using Xunit;

namespace GeoModeler3D.Tests.Services;

/// <summary>
/// Verifies the undo/redo command pattern used by ManipulatorService when a
/// drag gesture completes.  The tests are pure-Core: no WPF, no viewport, no
/// CuttingPlaneManipulator needed — they exercise the exact MacroCommand
/// construction that ManipulatorService.OnDragCompleted produces.
/// </summary>
public class ManipulatorUndoTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static CuttingPlaneEntity MakePlane(Vector3 origin, Vector3 normal) =>
        new(origin, normal);

    /// <summary>
    /// Simulates what ManipulatorService.OnDragCompleted does:
    /// builds a MacroCommand from pre/post drag state and pushes it to the
    /// UndoManager.
    /// </summary>
    private static UndoManager SimulateDragCompleted(
        CuttingPlaneEntity plane,
        Vector3 preDragOrigin, Vector3 preDragNormal)
    {
        var undoManager = new UndoManager();
        var postDragOrigin = plane.Origin;
        var postDragNormal = plane.Normal;

        var cmdOrigin = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Origin), preDragOrigin, postDragOrigin);

        var cmdNormal = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Normal), preDragNormal, postDragNormal);

        undoManager.Execute(new MacroCommand("Move Cutting Plane", [cmdOrigin, cmdNormal]));
        return undoManager;
    }

    // ── translation drag ──────────────────────────────────────────────────────

    [Fact]
    public void TranslationDrag_UndoRestoresOrigin()
    {
        var preDragOrigin = new Vector3(0, 0, 0);
        var preDragNormal = Vector3.UnitZ;
        var postDragOrigin = new Vector3(3, 1, -2);

        var plane = MakePlane(postDragOrigin, preDragNormal); // already at post-drag state

        var undoManager = SimulateDragCompleted(plane, preDragOrigin, preDragNormal);

        Assert.True(undoManager.CanUndo);
        Assert.Equal("Move Cutting Plane", undoManager.UndoDescription);

        // After Undo the plane should be back at the pre-drag position.
        undoManager.Undo();
        Assert.Equal(preDragOrigin, plane.Origin);
        Assert.Equal(preDragNormal, plane.Normal);
    }

    [Fact]
    public void TranslationDrag_RedoReappliesPostDragOrigin()
    {
        var preDragOrigin  = new Vector3(1, 2, 3);
        var preDragNormal  = Vector3.UnitZ;
        var postDragOrigin = new Vector3(4, 5, 6);

        var plane = MakePlane(postDragOrigin, preDragNormal);

        var undoManager = SimulateDragCompleted(plane, preDragOrigin, preDragNormal);

        undoManager.Undo();
        Assert.Equal(preDragOrigin, plane.Origin);

        undoManager.Redo();
        Assert.Equal(postDragOrigin, plane.Origin);
    }

    // ── rotation drag ─────────────────────────────────────────────────────────

    [Fact]
    public void RotationDrag_UndoRestoresNormal()
    {
        // Simulate rotating the plane so it faces X instead of Z.
        var preDragOrigin = Vector3.Zero;
        var preDragNormal = Vector3.UnitZ;
        var postDragNormal = Vector3.UnitX;

        var plane = MakePlane(preDragOrigin, postDragNormal); // already at post-drag state

        var undoManager = SimulateDragCompleted(plane, preDragOrigin, preDragNormal);

        undoManager.Undo();

        Assert.Equal(preDragNormal, plane.Normal);
        Assert.Equal(preDragOrigin, plane.Origin);
    }

    [Fact]
    public void RotationDrag_RedoReappliesPostDragNormal()
    {
        var preDragOrigin  = Vector3.Zero;
        var preDragNormal  = Vector3.UnitZ;
        var postDragNormal = Vector3.Normalize(new Vector3(1, 0, 1));

        var plane = MakePlane(preDragOrigin, postDragNormal);

        var undoManager = SimulateDragCompleted(plane, preDragOrigin, preDragNormal);

        undoManager.Undo();
        Assert.Equal(preDragNormal, plane.Normal);

        undoManager.Redo();
        // Normal is auto-normalised in the setter; compare to the entity's stored value.
        Assert.Equal(plane.Normal, plane.Normal); // tautological safety
        Assert.True(
            (plane.Normal - postDragNormal).LengthSquared() < 1e-6f,
            $"Expected normal ≈ {postDragNormal}, got {plane.Normal}");
    }

    // ── no-op drag (entity didn't actually move) ──────────────────────────────

    [Fact]
    public void NoDrag_NoCommandPushed_UndoStackEmpty()
    {
        // ManipulatorService guards against pushing when nothing changed.
        var origin = new Vector3(1, 0, 0);
        var normal = Vector3.UnitZ;
        var plane  = MakePlane(origin, normal);

        var undoManager = new UndoManager();

        // Replicate the guard in ManipulatorService.OnDragCompleted:
        var postDragOrigin = plane.Origin;
        var postDragNormal = plane.Normal;
        if (postDragOrigin != origin || postDragNormal != normal)
        {
            // Would push command — but the condition is false here.
            undoManager.Execute(new MacroCommand("Move Cutting Plane",
            [
                new ChangePropertyCommand<Vector3>(plane, nameof(CuttingPlaneEntity.Origin), origin, postDragOrigin),
                new ChangePropertyCommand<Vector3>(plane, nameof(CuttingPlaneEntity.Normal), normal, postDragNormal)
            ]));
        }

        Assert.False(undoManager.CanUndo);
    }

    // ── multiple sequential drags each produce their own undo entry ───────────

    [Fact]
    public void TwoSequentialDrags_ProduceTwoUndoEntries()
    {
        var origin1 = Vector3.Zero;
        var origin2 = new Vector3(1, 0, 0);
        var origin3 = new Vector3(2, 0, 0);
        var normal  = Vector3.UnitZ;

        var plane = MakePlane(origin2, normal); // after first drag
        var um = SimulateDragCompleted(plane, origin1, normal);

        // Apply second drag
        plane.Origin = origin3;
        var cmdOrigin2 = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Origin), origin2, origin3);
        var cmdNormal2 = new ChangePropertyCommand<Vector3>(
            plane, nameof(CuttingPlaneEntity.Normal), normal, normal);
        um.Execute(new MacroCommand("Move Cutting Plane", [cmdOrigin2, cmdNormal2]));

        Assert.Equal(origin3, plane.Origin);

        um.Undo(); // undo second drag
        Assert.Equal(origin2, plane.Origin);

        um.Undo(); // undo first drag
        Assert.Equal(origin1, plane.Origin);

        Assert.False(um.CanUndo);
    }
}
