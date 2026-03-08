using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering.EntityRenderers;

public class TriangleEntityRenderer : IEntityRenderer
{
    public Type SupportedEntityType => typeof(TriangleEntity);

    public Visual3D CreateVisual(IGeometricEntity entity)
    {
        var triangle = (TriangleEntity)entity;
        var visual = new ModelVisual3D();
        BuildMesh(triangle, visual);
        return visual;
    }

    public void UpdateVisual(IGeometricEntity entity, Visual3D visual)
    {
        BuildMesh((TriangleEntity)entity, (ModelVisual3D)visual);
    }

    public void DisposeVisual(Visual3D visual) { }

    private static void BuildMesh(TriangleEntity triangle, ModelVisual3D visual)
    {
        var mesh = new MeshGeometry3D();
        mesh.Positions.Add(triangle.Vertex0.ToPoint3D());
        mesh.Positions.Add(triangle.Vertex1.ToPoint3D());
        mesh.Positions.Add(triangle.Vertex2.ToPoint3D());
        // Front face
        mesh.TriangleIndices.Add(0);
        mesh.TriangleIndices.Add(1);
        mesh.TriangleIndices.Add(2);
        // Back face
        mesh.TriangleIndices.Add(2);
        mesh.TriangleIndices.Add(1);
        mesh.TriangleIndices.Add(0);

        var material = MaterialHelper.CreateMaterial(triangle.Color.ToWpfColor());
        visual.Content = new GeometryModel3D(mesh, material) { BackMaterial = material };
    }
}
