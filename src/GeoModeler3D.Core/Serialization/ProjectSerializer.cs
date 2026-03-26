using System.Numerics;
using System.Text.Json;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Serialization;

/// <summary>Serializes and deserializes entire project files to human-readable JSON.</summary>
public class ProjectSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Save(SceneManager scene, string filePath)
    {
        var visitor = new EntitySerializationVisitor();
        var entityList = new List<Dictionary<string, object?>>();

        foreach (var entity in scene.Entities)
        {
            entity.Accept(visitor);
            entityList.Add(visitor.Result);
        }

        var root = new Dictionary<string, object?>
        {
            ["version"] = ProjectFileSchema.CurrentVersion,
            ["savedAt"] = DateTime.UtcNow.ToString("o"),
            ["entities"] = entityList
        };

        var json = JsonSerializer.Serialize(root, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public void Load(SceneManager scene, string filePath)
    {
        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var version = root.GetProperty("version").GetInt32();
        if (version > ProjectFileSchema.CurrentVersion)
            throw new InvalidOperationException(
                $"File version {version} is newer than supported version {ProjectFileSchema.CurrentVersion}.");

        scene.Clear();

        var entities = root.GetProperty("entities");
        foreach (var elem in entities.EnumerateArray())
        {
            var entity = DeserializeEntity(elem);
            if (entity is not null)
                scene.Add(entity);
        }
    }

    private static IGeometricEntity? DeserializeEntity(JsonElement elem)
    {
        var type = elem.GetProperty("type").GetString();
        var id = Guid.Parse(elem.GetProperty("id").GetString()!);
        var name = elem.GetProperty("name").GetString() ?? "Unnamed";
        var color = EntityColor.FromHex(elem.GetProperty("color").GetString() ?? "#FFFFFFFF");
        var isVisible = elem.GetProperty("isVisible").GetBoolean();
        var layer = elem.GetProperty("layer").GetString() ?? "Default";

        EntityBase? entity = type switch
        {
            "Point" => DeserializePoint(elem),
            "Triangle" => DeserializeTriangle(elem),
            "Circle" => DeserializeCircle(elem),
            "Sphere" => DeserializeSphere(elem),
            "Cylinder" => DeserializeCylinder(elem),
            "Cone" => DeserializeCone(elem),
            "Torus" => DeserializeTorus(elem),
            "Mesh" => DeserializeMesh(elem),
            "CuttingPlane" => DeserializeCuttingPlane(elem),
            "ContourCurve" => DeserializeContourCurve(elem),
            _ => null
        };

        if (entity is null) return null;

        // Apply common metadata. Use reflection to set the readonly Id via the Guid constructor.
        // EntityBase already has a Guid-accepting constructor, but we constructed via the simpler
        // constructor above. Instead, set Name/Color/IsVisible/Layer directly.
        entity.Name = name;
        entity.Color = color;
        entity.IsVisible = isVisible;
        entity.Layer = layer;

        return entity;
    }

    private static Vector3 ReadVec3(JsonElement elem)
    {
        return new Vector3(
            elem.GetProperty("x").GetSingle(),
            elem.GetProperty("y").GetSingle(),
            elem.GetProperty("z").GetSingle());
    }

    private static PointEntity DeserializePoint(JsonElement e) =>
        new(ReadVec3(e.GetProperty("position")));

    private static TriangleEntity DeserializeTriangle(JsonElement e) =>
        new(ReadVec3(e.GetProperty("vertex0")),
            ReadVec3(e.GetProperty("vertex1")),
            ReadVec3(e.GetProperty("vertex2")));

    private static CircleEntity DeserializeCircle(JsonElement e)
    {
        var circle = new CircleEntity(
            ReadVec3(e.GetProperty("center")),
            ReadVec3(e.GetProperty("normal")),
            e.GetProperty("radius").GetDouble());
        if (e.TryGetProperty("segmentCount", out var seg))
            circle.SegmentCount = seg.GetInt32();
        return circle;
    }

    private static SphereEntity DeserializeSphere(JsonElement e) =>
        new(ReadVec3(e.GetProperty("center")),
            e.GetProperty("radius").GetDouble());

    private static CylinderEntity DeserializeCylinder(JsonElement e) =>
        new(ReadVec3(e.GetProperty("baseCenter")),
            ReadVec3(e.GetProperty("axis")),
            e.GetProperty("radius").GetDouble(),
            e.GetProperty("height").GetDouble());

    private static ConeEntity DeserializeCone(JsonElement e) =>
        new(ReadVec3(e.GetProperty("baseCenter")),
            ReadVec3(e.GetProperty("axis")),
            e.GetProperty("baseRadius").GetDouble(),
            e.GetProperty("height").GetDouble());

    private static TorusEntity DeserializeTorus(JsonElement e) =>
        new(ReadVec3(e.GetProperty("center")),
            ReadVec3(e.GetProperty("normal")),
            e.GetProperty("majorRadius").GetDouble(),
            e.GetProperty("minorRadius").GetDouble());

    private static MeshEntity DeserializeMesh(JsonElement e)
    {
        var posArray = e.GetProperty("positions");
        var floats = new List<float>();
        foreach (var f in posArray.EnumerateArray())
            floats.Add(f.GetSingle());

        var positions = new Vector3[floats.Count / 3];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = new Vector3(floats[i * 3], floats[i * 3 + 1], floats[i * 3 + 2]);

        return new MeshEntity(positions);
    }

    private static CuttingPlaneEntity DeserializeCuttingPlane(JsonElement e)
    {
        var cp = new CuttingPlaneEntity(
            ReadVec3(e.GetProperty("origin")),
            ReadVec3(e.GetProperty("normal")));
        if (e.TryGetProperty("displayWidth", out var dw)) cp.DisplayWidth = dw.GetDouble();
        if (e.TryGetProperty("displayHeight", out var dh)) cp.DisplayHeight = dh.GetDouble();
        if (e.TryGetProperty("opacity", out var op)) cp.Opacity = op.GetDouble();
        if (e.TryGetProperty("isCappingEnabled", out var cap)) cp.IsCappingEnabled = cap.GetBoolean();
        if (e.TryGetProperty("targetEntityIds", out var targets))
        {
            foreach (var tid in targets.EnumerateArray())
                cp.TargetEntityIds.Add(Guid.Parse(tid.GetString()!));
        }
        return cp;
    }

    private static ContourCurveEntity DeserializeContourCurve(JsonElement e)
    {
        var points = new List<Vector3>();
        foreach (var pt in e.GetProperty("points").EnumerateArray())
            points.Add(ReadVec3(pt));

        var contour = new ContourCurveEntity(
            points,
            Guid.Parse(e.GetProperty("sourcePlaneId").GetString()!),
            Guid.Parse(e.GetProperty("sourceEntityId").GetString()!));

        if (e.TryGetProperty("isClosed", out var closed)) contour.IsClosed = closed.GetBoolean();
        if (e.TryGetProperty("conicType", out var ct) && ct.ValueKind == JsonValueKind.String)
            contour.ConicType = Enum.Parse<ConicSectionType>(ct.GetString()!);

        return contour;
    }
}
