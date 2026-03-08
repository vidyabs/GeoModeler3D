using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports triangle meshes from STL files.</summary>
public class StlImporter : IFileImporter
{
    public string FormatName => "STL";
    public string FileFilter => "STL Files (*.stl)|*.stl";

    public ImportValidationResult Validate(string filePath)
    {
        // TODO: validate STL format (ASCII or binary)
        return new ImportValidationResult(true);
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        // TODO: parse STL and create TriangleEntity for each facet
        return [];
    }
}
