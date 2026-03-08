using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public abstract class EntityBase : IGeometricEntity
{
    private string _name;
    private EntityColor _color;
    private bool _isVisible = true;
    private string _layer = "Default";

    protected EntityBase(string name, EntityColor color)
    {
        Id = Guid.NewGuid();
        _name = name;
        _color = color;
    }

    protected EntityBase(Guid id, string name, EntityColor color)
    {
        Id = id;
        _name = name;
        _color = color;
    }

    public Guid Id { get; }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public EntityColor Color
    {
        get => _color;
        set => SetField(ref _color, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public string Layer
    {
        get => _layer;
        set => SetField(ref _layer, value);
    }

    public BoundingBox3D BoundingBox => ComputeBoundingBox();

    public abstract void Transform(Matrix4x4 matrix);
    public abstract IGeometricEntity Clone();
    public abstract void Accept(IEntityVisitor visitor);
    protected abstract BoundingBox3D ComputeBoundingBox();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void CopyMetadataTo(EntityBase target)
    {
        target._name = _name;
        target._color = _color;
        target._isVisible = _isVisible;
        target._layer = _layer;
    }
}
