using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Export;

/// <summary>Exports point clouds and meshes to PLY format.</summary>
public class PlyExporter : IFileExporter
{
    public string FormatName => "PLY";
    public string FileFilter => "PLY Files (*.ply)|*.ply";

    public void Export(IReadOnlyList<IGeometricEntity> entities, string filePath)
    {
        // TODO: write PLY header and vertex/face data
    }
}
