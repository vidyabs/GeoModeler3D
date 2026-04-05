using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Serialization;

/// <summary>
/// Visits an entity and produces a property dictionary suitable for JSON serialization.
/// </summary>
public class EntitySerializationVisitor : IEntityVisitor
{
    public Dictionary<string, object?> Result { get; private set; } = new();

    private void SetCommon(EntityBase entity, string typeName)
    {
        Result = new Dictionary<string, object?>
        {
            ["type"] = typeName,
            ["id"] = entity.Id.ToString(),
            ["name"] = entity.Name,
            ["color"] = entity.Color.ToHex(),
            ["isVisible"] = entity.IsVisible,
            ["layer"] = entity.Layer
        };
    }

    private static Dictionary<string, float> Vec3(Vector3 v) =>
        new() { ["x"] = v.X, ["y"] = v.Y, ["z"] = v.Z };

    public void Visit(PointEntity entity)
    {
        SetCommon(entity, "Point");
        Result["position"] = Vec3(entity.Position);
    }

    public void Visit(TriangleEntity entity)
    {
        SetCommon(entity, "Triangle");
        Result["vertex0"] = Vec3(entity.Vertex0);
        Result["vertex1"] = Vec3(entity.Vertex1);
        Result["vertex2"] = Vec3(entity.Vertex2);
    }

    public void Visit(CircleEntity entity)
    {
        SetCommon(entity, "Circle");
        Result["center"] = Vec3(entity.Center);
        Result["normal"] = Vec3(entity.Normal);
        Result["radius"] = entity.Radius;
        Result["segmentCount"] = entity.SegmentCount;
    }

    public void Visit(SphereEntity entity)
    {
        SetCommon(entity, "Sphere");
        Result["center"] = Vec3(entity.Center);
        Result["radius"] = entity.Radius;
    }

    public void Visit(CylinderEntity entity)
    {
        SetCommon(entity, "Cylinder");
        Result["baseCenter"] = Vec3(entity.BaseCenter);
        Result["axis"] = Vec3(entity.Axis);
        Result["radius"] = entity.Radius;
        Result["height"] = entity.Height;
    }

    public void Visit(ConeEntity entity)
    {
        SetCommon(entity, "Cone");
        Result["baseCenter"] = Vec3(entity.BaseCenter);
        Result["axis"] = Vec3(entity.Axis);
        Result["baseRadius"] = entity.BaseRadius;
        Result["height"] = entity.Height;
    }

    public void Visit(TorusEntity entity)
    {
        SetCommon(entity, "Torus");
        Result["center"] = Vec3(entity.Center);
        Result["normal"] = Vec3(entity.Normal);
        Result["majorRadius"] = entity.MajorRadius;
        Result["minorRadius"] = entity.MinorRadius;
    }

    public void Visit(MeshEntity entity)
    {
        SetCommon(entity, "Mesh");
        // Serialize as flat float array [x0,y0,z0, x1,y1,z1, ...]
        Result["positions"] = entity.Positions
            .SelectMany(v => new[] { v.X, v.Y, v.Z })
            .ToList();
    }

    public void Visit(VectorEntity entity)
    {
        SetCommon(entity, "Vector");
        Result["origin"] = Vec3(entity.Origin);
        Result["direction"] = Vec3(entity.Direction);
    }

    public void Visit(PlaneEntity entity)
    {
        SetCommon(entity, "Plane");
        Result["origin"] = Vec3(entity.Origin);
        Result["normal"] = Vec3(entity.Normal);
        Result["displaySize"] = entity.DisplaySize;
    }

    public void Visit(CuttingPlaneEntity entity)
    {
        SetCommon(entity, "CuttingPlane");
        Result["origin"] = Vec3(entity.Origin);
        Result["normal"] = Vec3(entity.Normal);
        Result["displayWidth"] = entity.DisplayWidth;
        Result["displayHeight"] = entity.DisplayHeight;
        Result["opacity"] = entity.Opacity;
        Result["isCappingEnabled"] = entity.IsCappingEnabled;
        Result["targetEntityIds"] = entity.TargetEntityIds.Select(id => id.ToString()).ToList();
    }

    public void Visit(ContourCurveEntity entity)
    {
        SetCommon(entity, "ContourCurve");
        Result["points"] = entity.Points.Select(Vec3).ToList();
        Result["sourcePlaneId"] = entity.SourcePlaneId.ToString();
        Result["sourceEntityId"] = entity.SourceEntityId.ToString();
        Result["isClosed"] = entity.IsClosed;
        Result["conicType"] = entity.ConicType?.ToString();
    }
}
