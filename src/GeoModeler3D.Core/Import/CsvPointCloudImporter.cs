using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports point clouds from CSV files (X,Y,Z per line).</summary>
public class CsvPointCloudImporter : IFileImporter
{
    public string FormatName => "CSV Point Cloud";
    public string FileFilter => "CSV Files (*.csv)|*.csv";

    public ImportValidationResult Validate(string filePath)
    {
        // TODO: validate CSV structure
        return new ImportValidationResult(true);
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        // TODO: parse CSV and create PointEntity for each row
        return [];
    }
}
