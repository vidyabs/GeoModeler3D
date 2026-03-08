using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Export;

/// <summary>Exports geometry to Wavefront OBJ format.</summary>
public class ObjExporter : IFileExporter
{
    public string FormatName => "OBJ";
    public string FileFilter => "OBJ Files (*.obj)|*.obj";

    public void Export(IReadOnlyList<IGeometricEntity> entities, string filePath)
    {
        // TODO: write vertices and faces in OBJ format
    }
}
