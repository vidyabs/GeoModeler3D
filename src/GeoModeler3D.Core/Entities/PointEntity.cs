using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class PointEntity : EntityBase
{
    private Vector3 _position;

    public PointEntity(Vector3 position, string name = "Point", EntityColor? color = null)
        : base(name, color ?? EntityColor.White)
    {
        _position = position;
    }

    private PointEntity(Guid id, Vector3 position, string name, EntityColor color)
        : base(id, name, color)
    {
        _position = position;
    }

    public Vector3 Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        Position = Vector3.Transform(_position, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new PointEntity(Guid.NewGuid(), _position, Name, Color);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox() => BoundingBox3D.FromPoint(_position);
}
