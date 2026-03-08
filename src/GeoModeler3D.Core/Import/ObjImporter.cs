using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports geometry from Wavefront OBJ files.</summary>
public class ObjImporter : IFileImporter
{
    public string FormatName => "OBJ";
    public string FileFilter => "OBJ Files (*.obj)|*.obj";

    public ImportValidationResult Validate(string filePath)
    {
        // TODO: validate OBJ format
        return new ImportValidationResult(true);
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        // TODO: parse OBJ vertices and faces
        return [];
    }
}
