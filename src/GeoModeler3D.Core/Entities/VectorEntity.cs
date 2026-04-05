using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class VectorEntity : EntityBase
{
    private Vector3 _origin;
    private Vector3 _direction; // NOT normalized — preserves magnitude

    public VectorEntity(Vector3 origin, Vector3 direction,
        string name = "Vector", EntityColor? color = null)
        : base(name, color ?? EntityColor.Yellow)
    {
        _origin = origin;
        _direction = direction;
    }

    private VectorEntity(Guid id, Vector3 origin, Vector3 direction, string name, EntityColor color)
        : base(id, name, color)
    {
        _origin = origin;
        _direction = direction;
    }

    public Vector3 Origin
    {
        get => _origin;
        set
        {
            SetField(ref _origin, value);
            OnPropertyChanged(nameof(Tip));
        }
    }

    public Vector3 Direction
    {
        get => _direction;
        set
        {
            SetField(ref _direction, value);
            OnPropertyChanged(nameof(Tip));
            OnPropertyChanged(nameof(Magnitude));
        }
    }

    public Vector3 Tip => _origin + _direction;
    public float Magnitude => _direction.Length();

    public override void Transform(Matrix4x4 matrix)
    {
        _origin = Vector3.Transform(_origin, matrix);
        _direction = Vector3.TransformNormal(_direction, matrix);
        OnPropertyChanged(nameof(Origin));
        OnPropertyChanged(nameof(Direction));
        OnPropertyChanged(nameof(Tip));
        OnPropertyChanged(nameof(Magnitude));
        OnPropertyChanged(nameof(BoundingBox));
    }

    public override IGeometricEntity Clone()
    {
        var clone = new VectorEntity(Guid.NewGuid(), _origin, _direction, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox() =>
        BoundingBox3D.FromPoints([_origin, Tip]);
}
