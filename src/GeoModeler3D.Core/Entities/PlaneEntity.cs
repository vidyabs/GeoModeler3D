using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class PlaneEntity : EntityBase
{
    private Vector3 _origin;
    private Vector3 _normal; // always normalized
    private double _displaySize = 5.0;

    public PlaneEntity(Vector3 origin, Vector3 normal,
        string name = "Plane", EntityColor? color = null)
        : base(name, color ?? new EntityColor(100, 149, 237))
    {
        _origin = origin;
        _normal = Vector3.Normalize(normal);
    }

    private PlaneEntity(Guid id, Vector3 origin, Vector3 normal, double displaySize, string name, EntityColor color)
        : base(id, name, color)
    {
        _origin = origin;
        _normal = Vector3.Normalize(normal);
        _displaySize = displaySize;
    }

    public Vector3 Origin
    {
        get => _origin;
        set
        {
            SetField(ref _origin, value);
            OnPropertyChanged(nameof(BoundingBox));
        }
    }

    public Vector3 Normal
    {
        get => _normal;
        set
        {
            SetField(ref _normal, Vector3.Normalize(value));
            OnPropertyChanged(nameof(BoundingBox));
        }
    }

    public double DisplaySize
    {
        get => _displaySize;
        set
        {
            SetField(ref _displaySize, value);
            OnPropertyChanged(nameof(BoundingBox));
        }
    }

    public override void Transform(Matrix4x4 matrix)
    {
        _origin = Vector3.Transform(_origin, matrix);
        _normal = Vector3.Normalize(Vector3.TransformNormal(_normal, matrix));
        OnPropertyChanged(nameof(Origin));
        OnPropertyChanged(nameof(Normal));
        OnPropertyChanged(nameof(BoundingBox));
    }

    public override IGeometricEntity Clone()
    {
        var clone = new PlaneEntity(Guid.NewGuid(), _origin, _normal, _displaySize, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        float h = (float)(_displaySize / 2);
        var (u, v) = ComputeTangents(_normal);
        return BoundingBox3D.FromPoints(
        [
            _origin - h * u - h * v,
            _origin + h * u - h * v,
            _origin + h * u + h * v,
            _origin - h * u + h * v
        ]);
    }

    public static (Vector3 u, Vector3 v) ComputeTangents(Vector3 normal)
    {
        var up = MathF.Abs(Vector3.Dot(normal, Vector3.UnitZ)) < 0.99f
            ? Vector3.UnitZ
            : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(normal, up));
        var v = Vector3.Cross(normal, u);
        return (u, v);
    }
}
