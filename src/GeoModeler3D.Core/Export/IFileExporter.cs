using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Export;

/// <summary>Interface for file format exporters.</summary>
public interface IFileExporter
{
    string FormatName { get; }
    string FileFilter { get; }
    void Export(IReadOnlyList<IGeometricEntity> entities, string filePath);
}
