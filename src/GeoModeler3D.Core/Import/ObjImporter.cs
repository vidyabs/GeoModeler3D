using System.Globalization;
using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports geometry from Wavefront OBJ files.</summary>
public class ObjImporter : IFileImporter
{
    public string FormatName => "OBJ";
    public string FileFilter => "OBJ Files (*.obj)|*.obj";

    public ImportValidationResult Validate(string filePath)
    {
        if (!File.Exists(filePath))
            return new ImportValidationResult(false, "File not found.");

        bool hasVertex = false, hasFace = false;
        foreach (var line in File.ReadLines(filePath))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("v ", StringComparison.Ordinal)) hasVertex = true;
            else if (trimmed.StartsWith("f ", StringComparison.Ordinal)) hasFace = true;
            if (hasVertex && hasFace) break;
        }

        if (!hasVertex || !hasFace)
            return new ImportValidationResult(false, "No vertex/face data found in OBJ file.");

        long fileSize = new FileInfo(filePath).Length;
        return new ImportValidationResult(true, null, null, fileSize);
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        var vertices = new List<Vector3>();
        var result = new List<IGeometricEntity>();

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("v ", StringComparison.Ordinal))
            {
                vertices.Add(ParseVertex(line[2..]));
            }
            else if (line.StartsWith("f ", StringComparison.Ordinal))
            {
                var indices = ParseFaceIndices(line[2..], vertices.Count);
                // Fan-triangulate: (0, i, i+1) for i in 1..N-2
                for (int i = 1; i < indices.Count - 1; i++)
                {
                    result.Add(new TriangleEntity(
                        vertices[indices[0]],
                        vertices[indices[i]],
                        vertices[indices[i + 1]]));
                }
            }
            // skip: #, mtllib, usemtl, vt, vn, o, g, s, l
        }
        return result;
    }

    // ---------------------------------------------------------------

    private static Vector3 ParseVertex(string s)
    {
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Parses OBJ face tokens like "1", "1/2", "1/2/3", "1//3".
    /// Returns 0-based indices. Handles negative (relative) references.
    /// </summary>
    private static List<int> ParseFaceIndices(string s, int vertexCount)
    {
        var result = new List<int>();
        foreach (var token in s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            // Take only the vertex index part (before any '/')
            var slash = token.IndexOf('/');
            var indexStr = slash >= 0 ? token[..slash] : token;
            if (!int.TryParse(indexStr, out int idx)) continue;

            // OBJ is 1-indexed; negative means relative from end
            int zeroBasedIdx = idx > 0 ? idx - 1 : vertexCount + idx;
            result.Add(zeroBasedIdx);
        }
        return result;
    }
}
