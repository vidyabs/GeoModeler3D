using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class CircleEntity : EntityBase
{
    private Vector3 _center;
    private Vector3 _normal;
    private double _radius;
    private int _segmentCount = MathConstants.DefaultSegmentCount;

    public CircleEntity(Vector3 center, Vector3 normal, double radius,
        string name = "Circle", EntityColor? color = null)
        : base(name, color ?? EntityColor.Green)
    {
        _center = center;
        _normal = Vector3.Normalize(normal);
        _radius = radius;
    }

    public Vector3 Center
    {
        get => _center;
        set => SetField(ref _center, value);
    }

    public Vector3 Normal
    {
        get => _normal;
        set => SetField(ref _normal, Vector3.Normalize(value));
    }

    public double Radius
    {
        get => _radius;
        set => SetField(ref _radius, value);
    }

    public int SegmentCount
    {
        get => _segmentCount;
        set => SetField(ref _segmentCount, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        Center = Vector3.Transform(_center, matrix);
        Normal = Vector3.TransformNormal(_normal, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new CircleEntity(_center, _normal, _radius, Name, Color)
        {
            SegmentCount = _segmentCount
        };
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
