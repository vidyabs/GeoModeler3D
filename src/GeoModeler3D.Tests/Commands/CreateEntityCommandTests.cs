using System.Numerics;
using GeoModeler3D.Core.Commands;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using Xunit;

namespace GeoModeler3D.Tests.Commands;

public class CreateEntityCommandTests
{
    private readonly SceneManager _scene = new();

    [Fact]
    public void Execute_AddsEntityToScene()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0, "TestSphere");
        var cmd = new CreateEntityCommand(_scene, entity);

        cmd.Execute();

        Assert.Single(_scene.Entities);
        Assert.Same(entity, _scene.Entities[0]);
    }

    [Fact]
    public void Undo_RemovesEntityFromScene()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        var cmd = new CreateEntityCommand(_scene, entity);
        cmd.Execute();

        cmd.Undo();

        Assert.Empty(_scene.Entities);
    }

    [Fact]
    public void Description_ContainsEntityName()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0, "Ball");
        var cmd = new CreateEntityCommand(_scene, entity);

        Assert.Equal("Create Ball", cmd.Description);
    }

    [Fact]
    public void ExecuteUndoExecute_RestoresEntity()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        var cmd = new CreateEntityCommand(_scene, entity);

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();

        Assert.Single(_scene.Entities);
        Assert.Same(entity, _scene.Entities[0]);
    }
}

public class DeleteEntityCommandTests
{
    private readonly SceneManager _scene = new();

    [Fact]
    public void Execute_RemovesEntityFromScene()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(entity);

        var cmd = new DeleteEntityCommand(_scene, entity);
        cmd.Execute();

        Assert.Empty(_scene.Entities);
    }

    [Fact]
    public void Undo_RestoresEntityAtOriginalIndex()
    {
        var e1 = new PointEntity(Vector3.Zero, "P1");
        var e2 = new SphereEntity(Vector3.One, 1.0, "S1");
        var e3 = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 1.0, 2.0, "C1");
        _scene.Add(e1);
        _scene.Add(e2);
        _scene.Add(e3);

        var cmd = new DeleteEntityCommand(_scene, e2);
        cmd.Execute();

        Assert.Equal(2, _scene.Entities.Count);
        Assert.DoesNotContain(e2, _scene.Entities);

        cmd.Undo();

        Assert.Equal(3, _scene.Entities.Count);
        Assert.Same(e2, _scene.Entities[1]);
    }

    [Fact]
    public void Description_ContainsEntityName()
    {
        var entity = new SphereEntity(Vector3.Zero, 1.0, "MySphere");
        _scene.Add(entity);
        var cmd = new DeleteEntityCommand(_scene, entity);

        Assert.Equal("Delete MySphere", cmd.Description);
    }
}

public class TransformEntityCommandTests
{
    [Fact]
    public void Execute_AppliesTransform()
    {
        var point = new PointEntity(new Vector3(1, 2, 3));
        var matrix = Matrix4x4.CreateTranslation(10, 0, 0);
        var cmd = new TransformEntityCommand(point, matrix);

        cmd.Execute();

        Assert.Equal(11, point.Position.X, 0.001f);
    }

    [Fact]
    public void Undo_ReversesTransform()
    {
        var point = new PointEntity(new Vector3(1, 2, 3));
        var matrix = Matrix4x4.CreateTranslation(10, 0, 0);
        var cmd = new TransformEntityCommand(point, matrix);

        cmd.Execute();
        cmd.Undo();

        Assert.Equal(1, point.Position.X, 0.001f);
        Assert.Equal(2, point.Position.Y, 0.001f);
        Assert.Equal(3, point.Position.Z, 0.001f);
    }
}

public class MacroCommandTests
{
    [Fact]
    public void Execute_RunsAllChildren()
    {
        var scene = new SceneManager();
        var e1 = new PointEntity(Vector3.Zero, "P1");
        var e2 = new SphereEntity(Vector3.One, 1.0, "S1");

        var macro = new MacroCommand("Create two entities",
        [
            new CreateEntityCommand(scene, e1),
            new CreateEntityCommand(scene, e2)
        ]);

        macro.Execute();

        Assert.Equal(2, scene.Entities.Count);
    }

    [Fact]
    public void Undo_ReversesAllChildrenInReverseOrder()
    {
        var scene = new SceneManager();
        var e1 = new PointEntity(Vector3.Zero, "P1");
        var e2 = new SphereEntity(Vector3.One, 1.0, "S1");

        var macro = new MacroCommand("Create two entities",
        [
            new CreateEntityCommand(scene, e1),
            new CreateEntityCommand(scene, e2)
        ]);

        macro.Execute();
        macro.Undo();

        Assert.Empty(scene.Entities);
    }

    [Fact]
    public void Add_ThenExecute_IncludesAddedCommand()
    {
        var scene = new SceneManager();
        var macro = new MacroCommand("Build up");

        macro.Add(new CreateEntityCommand(scene, new PointEntity(Vector3.Zero)));
        macro.Add(new CreateEntityCommand(scene, new PointEntity(Vector3.One)));
        macro.Execute();

        Assert.Equal(2, scene.Entities.Count);
    }
}

public class ChangePropertyCommandTests
{
    [Fact]
    public void Execute_ChangesProperty()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0, "S1");
        var cmd = new ChangePropertyCommand<double>(sphere, "Radius", 1.0, 5.0);

        cmd.Execute();

        Assert.Equal(5.0, sphere.Radius);
    }

    [Fact]
    public void Undo_RestoresOldValue()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0, "S1");
        var cmd = new ChangePropertyCommand<double>(sphere, "Radius", 1.0, 5.0);

        cmd.Execute();
        cmd.Undo();

        Assert.Equal(1.0, sphere.Radius);
    }

    [Fact]
    public void Description_ContainsPropertyAndEntityName()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0, "Ball");
        var cmd = new ChangePropertyCommand<double>(sphere, "Radius", 1.0, 5.0);

        Assert.Equal("Change Radius of Ball", cmd.Description);
    }
}
