using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class SphereEntity : EntityBase
{
    private Vector3 _center;
    private double _radius;

    public SphereEntity(Vector3 center, double radius,
        string name = "Sphere", EntityColor? color = null)
        : base(name, color ?? EntityColor.Blue)
    {
        _center = center;
        _radius = radius;
    }

    public Vector3 Center
    {
        get => _center;
        set => SetField(ref _center, value);
    }

    public double Radius
    {
        get => _radius;
        set => SetField(ref _radius, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        Center = Vector3.Transform(_center, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new SphereEntity(_center, _radius, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        var r = (float)_radius;
        return new BoundingBox3D(_center - new Vector3(r), _center + new Vector3(r));
    }
}
