using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Interface for file format importers.</summary>
public interface IFileImporter
{
    string FormatName { get; }
    string FileFilter { get; }
    ImportValidationResult Validate(string filePath);
    IReadOnlyList<IGeometricEntity> Import(string filePath);
}
