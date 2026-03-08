using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports point clouds and meshes from PLY files.</summary>
public class PlyImporter : IFileImporter
{
    public string FormatName => "PLY";
    public string FileFilter => "PLY Files (*.ply)|*.ply";

    public ImportValidationResult Validate(string filePath)
    {
        // TODO: validate PLY format
        return new ImportValidationResult(true);
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        // TODO: parse PLY vertices and faces
        return [];
    }
}
