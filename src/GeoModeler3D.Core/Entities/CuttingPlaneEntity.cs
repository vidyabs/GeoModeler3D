using System.Numerics;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Entities;

public class CuttingPlaneEntity : EntityBase
{
    private Vector3 _origin;
    private Vector3 _normal;
    private double _displayWidth = 10.0;
    private double _displayHeight = 10.0;
    private double _opacity = 0.3;
    private bool _isCappingEnabled;
    private ClipSide _clipSide;
    private double _gapDistance = 0.5;

    public CuttingPlaneEntity(Vector3 origin, Vector3 normal,
        string name = "CuttingPlane", EntityColor? color = null)
        : base(name, color ?? new EntityColor(0, 136, 255, 68))
    {
        _origin = origin;
        _normal = Vector3.Normalize(normal);
    }

    public Vector3 Origin
    {
        get => _origin;
        set => SetField(ref _origin, value);
    }

    public Vector3 Normal
    {
        get => _normal;
        set => SetField(ref _normal, Vector3.Normalize(value));
    }

    public double DisplayWidth
    {
        get => _displayWidth;
        set => SetField(ref _displayWidth, value);
    }

    public double DisplayHeight
    {
        get => _displayHeight;
        set => SetField(ref _displayHeight, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetField(ref _opacity, value);
    }

    public List<Guid> TargetEntityIds { get; } = [];

    public bool IsCappingEnabled
    {
        get => _isCappingEnabled;
        set => SetField(ref _isCappingEnabled, value);
    }

    public ClipSide ClipSide
    {
        get => _clipSide;
        set => SetField(ref _clipSide, value);
    }

    public double GapDistance
    {
        get => _gapDistance;
        set => SetField(ref _gapDistance, value);
    }

    public override void Transform(Matrix4x4 matrix)
    {
        Origin = Vector3.Transform(_origin, matrix);
        Normal = Vector3.TransformNormal(_normal, matrix);
    }

    public override IGeometricEntity Clone()
    {
        var clone = new CuttingPlaneEntity(_origin, _normal, Name, Color)
        {
            DisplayWidth = _displayWidth,
            DisplayHeight = _displayHeight,
            Opacity = _opacity,
            IsCappingEnabled = _isCappingEnabled,
            ClipSide = _clipSide,
            GapDistance = _gapDistance
        };
        clone.TargetEntityIds.AddRange(TargetEntityIds);
        CopyMetadataTo(clone);
        return clone;
    }

    public override void Accept(IEntityVisitor visitor) => visitor.Visit(this);

    protected override BoundingBox3D ComputeBoundingBox()
    {
        var halfSize = (float)System.Math.Max(_displayWidth, _displayHeight) / 2f;
        var offset = new Vector3(halfSize);
        return new BoundingBox3D(_origin - offset, _origin + offset);
    }
}
