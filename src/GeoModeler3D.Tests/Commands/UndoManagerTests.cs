using System.Numerics;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using Xunit;

namespace GeoModeler3D.Tests.Commands;

public class UndoManagerTests
{
    private readonly UndoManager _undoManager = new();
    private readonly SceneManager _scene = new();

    [Fact]
    public void Initially_CannotUndoOrRedo()
    {
        Assert.False(_undoManager.CanUndo);
        Assert.False(_undoManager.CanRedo);
        Assert.Null(_undoManager.UndoDescription);
        Assert.Null(_undoManager.RedoDescription);
    }

    [Fact]
    public void Execute_CommandIsExecuted()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        var cmd = new CreateEntityCommand(_scene, entity);

        _undoManager.Execute(cmd);

        Assert.Single(_scene.Entities);
        Assert.True(_undoManager.CanUndo);
        Assert.False(_undoManager.CanRedo);
    }

    [Fact]
    public void Undo_ReversesCommand()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        _undoManager.Execute(new CreateEntityCommand(_scene, entity));

        _undoManager.Undo();

        Assert.Empty(_scene.Entities);
        Assert.False(_undoManager.CanUndo);
        Assert.True(_undoManager.CanRedo);
    }

    [Fact]
    public void Redo_ReExecutesCommand()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        _undoManager.Execute(new CreateEntityCommand(_scene, entity));
        _undoManager.Undo();

        _undoManager.Redo();

        Assert.Single(_scene.Entities);
        Assert.True(_undoManager.CanUndo);
        Assert.False(_undoManager.CanRedo);
    }

    [Fact]
    public void Execute_ClearsRedoStack()
    {
        var e1 = new SphereEntity(Vector3.Zero, 1.0, "S1");
        var e2 = new PointEntity(Vector3.One, "P1");

        _undoManager.Execute(new CreateEntityCommand(_scene, e1));
        _undoManager.Undo();
        Assert.True(_undoManager.CanRedo);

        _undoManager.Execute(new CreateEntityCommand(_scene, e2));

        Assert.False(_undoManager.CanRedo);
    }

    [Fact]
    public void MultipleUndoRedo_WorksCorrectly()
    {
        var e1 = new SphereEntity(Vector3.Zero, 1.0, "S1");
        var e2 = new PointEntity(Vector3.One, "P1");
        var e3 = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 2.0, 5.0, "C1");

        _undoManager.Execute(new CreateEntityCommand(_scene, e1));
        _undoManager.Execute(new CreateEntityCommand(_scene, e2));
        _undoManager.Execute(new CreateEntityCommand(_scene, e3));
        Assert.Equal(3, _scene.Entities.Count);

        _undoManager.Undo();
        Assert.Equal(2, _scene.Entities.Count);

        _undoManager.Undo();
        Assert.Single(_scene.Entities);

        _undoManager.Redo();
        Assert.Equal(2, _scene.Entities.Count);

        _undoManager.Undo();
        _undoManager.Undo();
        Assert.Empty(_scene.Entities);
    }

    [Fact]
    public void MaxDepth_IsRespected()
    {
        _undoManager.MaxDepth = 3;

        for (int i = 0; i < 5; i++)
        {
            var entity = new PointEntity(new Vector3(i, 0, 0), $"P{i}");
            _undoManager.Execute(new CreateEntityCommand(_scene, entity));
        }

        // Only 3 undos should be possible
        int undoCount = 0;
        while (_undoManager.CanUndo)
        {
            _undoManager.Undo();
            undoCount++;
        }

        Assert.Equal(3, undoCount);
    }

    [Fact]
    public void Clear_EmptiesBothStacks()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        _undoManager.Execute(new CreateEntityCommand(_scene, entity));
        _undoManager.Undo();

        _undoManager.Clear();

        Assert.False(_undoManager.CanUndo);
        Assert.False(_undoManager.CanRedo);
    }

    [Fact]
    public void StackChanged_FiresOnExecute()
    {
        int fireCount = 0;
        _undoManager.StackChanged += () => fireCount++;

        _undoManager.Execute(new CreateEntityCommand(_scene, new PointEntity(Vector3.Zero)));

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void StackChanged_FiresOnUndoAndRedo()
    {
        _undoManager.Execute(new CreateEntityCommand(_scene, new PointEntity(Vector3.Zero)));

        int fireCount = 0;
        _undoManager.StackChanged += () => fireCount++;

        _undoManager.Undo();
        _undoManager.Redo();

        Assert.Equal(2, fireCount);
    }

    [Fact]
    public void Undo_WhenEmpty_DoesNothing()
    {
        _undoManager.Undo(); // Should not throw
        Assert.False(_undoManager.CanUndo);
    }

    [Fact]
    public void Redo_WhenEmpty_DoesNothing()
    {
        _undoManager.Redo(); // Should not throw
        Assert.False(_undoManager.CanRedo);
    }

    [Fact]
    public void UndoDescription_ReturnsTopCommandDescription()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0, "MySphere");
        _undoManager.Execute(new CreateEntityCommand(_scene, entity));

        Assert.Equal("Create MySphere", _undoManager.UndoDescription);
    }

    [Fact]
    public void RedoDescription_ReturnsTopCommandDescription()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0, "MySphere");
        _undoManager.Execute(new CreateEntityCommand(_scene, entity));
        _undoManager.Undo();

        Assert.Equal("Create MySphere", _undoManager.RedoDescription);
    }
}
