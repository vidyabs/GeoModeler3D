using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class ConeEntity : EntityBase
{
    private Vector3 _baseCenter;
    private Vector3 _axis;
    private double _baseRadius;
    private double _height;

    public ConeEntity(Vector3 baseCenter, Vector3 axis, double baseRadius, double height,
        string name = "Cone", EntityColor? color = null)
        : base(name, color ?? EntityColor.Red)
    {
        _baseCenter = baseCenter;
        _axis = Vector3.Normalize(axis);
        _baseRadius = baseRadius;
        _height = height;
    }

    public Vector3 BaseCenter
    {
        get => _baseCenter;
        set => SetField(ref _baseCenter, value);
    }

    public Vector3 Axis
    {
        get => _axis;
        set => SetField(ref _axis, Vector3.Normalize(value));
    }

    public double BaseRadius
    {
        get => _baseRadius;
        set { SetField(ref _baseRadius, value); OnPropertyChanged(nameof(HalfAngle)); }
    }

    public double Height
    {
        get => _height;
        set { SetField(ref _height, value); OnPropertyChanged(nameof(HalfAngle)); }
    }

    public double HalfAngle => System.Math.Atan2(_baseRadius, _height);

    public Vector3 Apex => _baseCenter + _axis * (float)_height;

    public override void Transform(Matrix4x4 matrix)
    {
        BaseCenter = Vector3.Transform(_baseCenter, matrix);
        Axis = Vector3.TransformNormal(_axis, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new ConeEntity(_baseCenter, _axis, _baseRadius, _height, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        var r = (float)_baseRadius;
        var apex = Apex;
        var offset = new Vector3(r);
        var b1 = new BoundingBox3D(_baseCenter - offset, _baseCenter + offset);
        return b1.Expand(apex);
    }
}
