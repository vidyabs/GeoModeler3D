using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Export;

/// <summary>Exports triangle meshes to STL format.</summary>
public class StlExporter : IFileExporter
{
    public string FormatName => "STL";
    public string FileFilter => "STL Files (*.stl)|*.stl";

    public void Export(IReadOnlyList<IGeometricEntity> entities, string filePath)
    {
        // TODO: tessellate entities and write STL
    }
}
