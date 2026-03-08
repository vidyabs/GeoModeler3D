using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;
using Xunit;

namespace GeoModeler3D.Tests.SceneGraph;

public class SceneManagerTests
{
    private readonly SceneManager _scene = new();

    [Fact]
    public void Add_IncreasesEntityCount()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);

        Assert.Single(_scene.Entities);
    }

    [Fact]
    public void Add_RaisesEntityAddedEvent()
    {
        IGeometricEntity? addedEntity = null;
        _scene.EntityAdded += e => addedEntity = e;

        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);

        Assert.Same(sphere, addedEntity);
    }

    [Fact]
    public void Remove_DecreasesEntityCount()
    {
        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);
        _scene.Remove(sphere.Id);

        Assert.Empty(_scene.Entities);
    }

    [Fact]
    public void Remove_RaisesEntityRemovedEvent()
    {
        Guid? removedId = null;
        _scene.EntityRemoved += id => removedId = id;

        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);
        _scene.Remove(sphere.Id);

        Assert.Equal(sphere.Id, removedId);
    }

    [Fact]
    public void GetById_ReturnsCorrectEntity()
    {
        var s1 = new SphereEntity(Vector3.Zero, 1.0, "S1");
        var s2 = new SphereEntity(Vector3.One, 2.0, "S2");
        _scene.Add(s1);
        _scene.Add(s2);

        var found = _scene.GetById(s2.Id);

        Assert.Same(s2, found);
    }

    [Fact]
    public void GetById_ReturnsNullForUnknownId()
    {
        Assert.Null(_scene.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void Clear_RemovesAllEntities()
    {
        _scene.Add(new SphereEntity(Vector3.Zero, 1.0));
        _scene.Add(new PointEntity(Vector3.One));
        _scene.Clear();

        Assert.Empty(_scene.Entities);
    }

    [Fact]
    public void Clear_RaisesEntityRemovedForEach()
    {
        var removedIds = new List<Guid>();
        _scene.EntityRemoved += id => removedIds.Add(id);

        var s1 = new SphereEntity(Vector3.Zero, 1.0);
        var s2 = new PointEntity(Vector3.One);
        _scene.Add(s1);
        _scene.Add(s2);
        _scene.Clear();

        Assert.Equal(2, removedIds.Count);
        Assert.Contains(s1.Id, removedIds);
        Assert.Contains(s2.Id, removedIds);
    }

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var s1 = new SphereEntity(Vector3.Zero, 1.0);
        var s2 = new PointEntity(Vector3.One);
        _scene.Add(s1);
        _scene.Add(s2);

        Assert.Equal(0, _scene.IndexOf(s1.Id));
        Assert.Equal(1, _scene.IndexOf(s2.Id));
    }

    [Fact]
    public void Insert_PlacesAtCorrectIndex()
    {
        var s1 = new SphereEntity(Vector3.Zero, 1.0, "First");
        var s2 = new SphereEntity(Vector3.One, 2.0, "Third");
        _scene.Add(s1);
        _scene.Add(s2);

        var s3 = new PointEntity(Vector3.UnitX, "Second");
        _scene.Insert(1, s3);

        Assert.Equal(3, _scene.Entities.Count);
        Assert.Same(s3, _scene.Entities[1]);
    }

    [Fact]
    public void EntityChanged_FiresWhenEntityPropertyChanges()
    {
        IGeometricEntity? changedEntity = null;
        _scene.EntityChanged += e => changedEntity = e;

        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);
        sphere.Radius = 5.0;

        Assert.Same(sphere, changedEntity);
    }

    [Fact]
    public void EntityChanged_DoesNotFireAfterRemoval()
    {
        bool fired = false;
        _scene.EntityChanged += _ => fired = true;

        var sphere = new SphereEntity(Vector3.Zero, 1.0);
        _scene.Add(sphere);
        _scene.Remove(sphere.Id);
        sphere.Radius = 5.0;

        Assert.False(fired);
    }
}
