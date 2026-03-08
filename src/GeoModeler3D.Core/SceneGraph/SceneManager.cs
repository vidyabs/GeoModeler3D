using System.Collections.ObjectModel;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.SceneGraph;

public class SceneManager
{
    public ObservableCollection<IGeometricEntity> Entities { get; } = [];

    public event Action<IGeometricEntity>? EntityAdded;
    public event Action<IGeometricEntity>? EntityChanged;
    public event Action<Guid>? EntityRemoved;

    public void Add(IGeometricEntity entity)
    {
        Entities.Add(entity);
        entity.PropertyChanged += OnEntityPropertyChanged;
        EntityAdded?.Invoke(entity);
    }

    public void Insert(int index, IGeometricEntity entity)
    {
        Entities.Insert(index, entity);
        entity.PropertyChanged += OnEntityPropertyChanged;
        EntityAdded?.Invoke(entity);
    }

    public void Remove(Guid id)
    {
        var entity = GetById(id);
        if (entity is null) return;
        entity.PropertyChanged -= OnEntityPropertyChanged;
        Entities.Remove(entity);
        EntityRemoved?.Invoke(id);
    }

    public IGeometricEntity? GetById(Guid id) =>
        Entities.FirstOrDefault(e => e.Id == id);

    public int IndexOf(Guid id)
    {
        for (int i = 0; i < Entities.Count; i++)
        {
            if (Entities[i].Id == id) return i;
        }
        return -1;
    }

    public void Clear()
    {
        foreach (var entity in Entities)
            entity.PropertyChanged -= OnEntityPropertyChanged;

        var ids = Entities.Select(e => e.Id).ToList();
        Entities.Clear();

        foreach (var id in ids)
            EntityRemoved?.Invoke(id);
    }

    private void OnEntityPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is IGeometricEntity entity)
            EntityChanged?.Invoke(entity);
    }
}
