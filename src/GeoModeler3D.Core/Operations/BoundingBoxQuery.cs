using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;
using GeoModeler3D.Core.SceneGraph;

namespace GeoModeler3D.Core.Operations;

/// <summary>Queries and computes bounding boxes for scene entities.</summary>
public class BoundingBoxQuery
{
    public BoundingBox3D ComputeSceneBounds(SceneManager scene)
    {
        if (scene.Entities.Count == 0)
            return new BoundingBox3D(System.Numerics.Vector3.Zero, System.Numerics.Vector3.Zero);

        var bounds = scene.Entities[0].BoundingBox;
        for (int i = 1; i < scene.Entities.Count; i++)
            bounds = bounds.Merge(scene.Entities[i].BoundingBox);
        return bounds;
    }
}
