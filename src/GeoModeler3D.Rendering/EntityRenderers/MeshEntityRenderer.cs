using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class MeshEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(MeshEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var visual = new ModelVisual3D();
        BuildMesh((MeshEntity)entity, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        BuildMesh((MeshEntity)entity, (ModelVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void BuildMesh(MeshEntity mesh, ModelVisual3D visual)
    {
        var geometry = new MeshGeometry3D();
        var positions = mesh.Positions;

        for (int i = 0; i + 2 < positions.Count; i += 3)
        {
            int baseIdx = geometry.Positions.Count;
            geometry.Positions.Add(positions[i].ToPoint3D());
            geometry.Positions.Add(positions[i + 1].ToPoint3D());
            geometry.Positions.Add(positions[i + 2].ToPoint3D());
            // Front face
            geometry.TriangleIndices.Add(baseIdx);
            geometry.TriangleIndices.Add(baseIdx + 1);
            geometry.TriangleIndices.Add(baseIdx + 2);
            // Back face
            geometry.TriangleIndices.Add(baseIdx + 2);
            geometry.TriangleIndices.Add(baseIdx + 1);
            geometry.TriangleIndices.Add(baseIdx);
        }

        var material = MaterialHelper.CreateMaterial(mesh.Color.ToWpfColor());
        visual.Content = new GeometryModel3D(geometry, material) { BackMaterial = material };
    }
}
