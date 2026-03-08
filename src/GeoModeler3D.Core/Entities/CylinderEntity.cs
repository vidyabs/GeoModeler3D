using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class CylinderEntity : EntityBase
{
    private Vector3 _baseCenter;
    private Vector3 _axis;
    private double _radius;
    private double _height;

    public CylinderEntity(Vector3 baseCenter, Vector3 axis, double radius, double height,
        string name = "Cylinder", EntityColor? color = null)
        : base(name, color ?? EntityColor.Orange)
    {
        _baseCenter = baseCenter;
        _axis = Vector3.Normalize(axis);
        _radius = radius;
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

    public double Radius
    {
        get => _radius;
        set => SetField(ref _radius, value);
    }

    public double Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }

    public Vector3 TopCenter => _baseCenter + _axis * (float)_height;

    public override void Transform(Matrix4x4 matrix)
    {
        BaseCenter = Vector3.Transform(_baseCenter, matrix);
        Axis = Vector3.TransformNormal(_axis, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new CylinderEntity(_baseCenter, _axis, _radius, _height, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        var r = (float)_radius;
        var top = TopCenter;
        var offset = new Vector3(r);
        var b1 = new BoundingBox3D(_baseCenter - offset, _baseCenter + offset);
        var b2 = new BoundingBox3D(top - offset, top + offset);
        return b1.Merge(b2);
    }
}
