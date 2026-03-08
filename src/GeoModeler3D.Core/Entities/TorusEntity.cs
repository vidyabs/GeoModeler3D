using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class TorusEntity : EntityBase
{
    private Vector3 _center;
    private Vector3 _normal;
    private double _majorRadius;
    private double _minorRadius;

    public TorusEntity(Vector3 center, Vector3 normal, double majorRadius, double minorRadius,
        string name = "Torus", EntityColor? color = null)
        : base(name, color ?? EntityColor.Magenta)
    {
        _center = center;
        _normal = Vector3.Normalize(normal);
        _majorRadius = majorRadius;
        _minorRadius = minorRadius;
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

    public double MajorRadius
    {
        get => _majorRadius;
        set => SetField(ref _majorRadius, value);
    }

    public double MinorRadius
    {
        get => _minorRadius;
        set => SetField(ref _minorRadius, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        Center = Vector3.Transform(_center, matrix);
        Normal = Vector3.TransformNormal(_normal, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new TorusEntity(_center, _normal, _majorRadius, _minorRadius, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        var outerR = (float)(_majorRadius + _minorRadius);
        var offset = new Vector3(outerR, outerR, (float)_minorRadius);
        return new BoundingBox3D(_center - offset, _center + offset);
    }
}
