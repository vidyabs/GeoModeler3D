using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports triangle meshes from VRML 2.0 (.wrl) files via IndexedFaceSet nodes.</summary>
public class WrlImporter : IFileImporter
{
    public string FormatName => "VRML";
    public string FileFilter => "VRML Files (*.wrl)|*.wrl";

    public ImportValidationResult Validate(string filePath)
    {
        if (!File.Exists(filePath))
            return new ImportValidationResult(false, "File not found.");

        try
        {
            // VRML 2.0 files must start with "#VRML V2.0" header
            string? firstLine = null;
            foreach (var line in File.ReadLines(filePath))
            {
                firstLine = line.Trim();
                if (firstLine.Length > 0) break;
            }

            if (firstLine is null ||
                !firstLine.Contains("VRML V2.0", StringComparison.OrdinalIgnoreCase))
            {
                return new ImportValidationResult(false,
                    "File does not begin with a VRML V2.0 header.");
            }

            long fileSize = new FileInfo(filePath).Length;
            return new ImportValidationResult(true, null, null, fileSize);
        }
        catch (Exception ex)
        {
            return new ImportValidationResult(false, ex.Message);
        }
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        var result = new List<IGeometricEntity>();
        string text = File.ReadAllText(filePath);

        // Find every IndexedFaceSet block and process it
        int searchFrom = 0;
        while (true)
        {
            int start = text.IndexOf("IndexedFaceSet", searchFrom, StringComparison.OrdinalIgnoreCase);
            if (start < 0) break;

            // Advance to opening brace of this node
            int braceStart = text.IndexOf('{', start);
            if (braceStart < 0) break;

            string block = ExtractBalancedBlock(text, braceStart);
            searchFrom = braceStart + block.Length;

            var vertices = ExtractCoordinates(block);
            if (vertices.Count == 0) continue;

            var faces = ExtractFaceIndices(block);
            foreach (var face in faces)
            {
                // Fan-triangulate: (0, i, i+1) for i in 1..N-2
                for (int i = 1; i < face.Count - 1; i++)
                {
                    int a = face[0], b = face[i], c = face[i + 1];
                    if (a >= 0 && a < vertices.Count &&
                        b >= 0 && b < vertices.Count &&
                        c >= 0 && c < vertices.Count)
                    {
                        result.Add(new TriangleEntity(vertices[a], vertices[b], vertices[c]));
                    }
                }
            }
        }
        return result;
    }

    // ---------------------------------------------------------------

    /// <summary>
    /// Returns the substring starting at <paramref name="openBrace"/> up to and including
    /// the matching closing brace (handles nesting).
    /// </summary>
    private static string ExtractBalancedBlock(string text, int openBrace)
    {
        int depth = 0;
        for (int i = openBrace; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return text[openBrace..(i + 1)];
            }
        }
        return text[openBrace..];
    }

    /// <summary>
    /// Parses the <c>coord Coordinate { point [ x y z, ... ] }</c> section.
    /// </summary>
    private static List<Vector3> ExtractCoordinates(string block)
    {
        var result = new List<Vector3>();

        // Find "point [" array
        int ptKeyword = block.IndexOf("point", StringComparison.OrdinalIgnoreCase);
        if (ptKeyword < 0) return result;

        int arrayStart = block.IndexOf('[', ptKeyword);
        int arrayEnd   = block.IndexOf(']', arrayStart > 0 ? arrayStart : 0);
        if (arrayStart < 0 || arrayEnd < 0) return result;

        string arrayContent = block[(arrayStart + 1)..arrayEnd];

        // Split on commas and whitespace; parse triplets of floats
        var tokens = Regex.Split(arrayContent, @"[\s,]+")
                          .Where(t => t.Length > 0)
                          .ToArray();

        for (int i = 0; i + 2 < tokens.Length; i += 3)
        {
            if (float.TryParse(tokens[i],     NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(tokens[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(tokens[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                result.Add(new Vector3(x, y, z));
            }
        }
        return result;
    }

    /// <summary>
    /// Parses the <c>coordIndex [ 0 1 2 -1 3 4 5 -1 ... ]</c> section.
    /// Returns one list of indices per face (-1 terminates each face).
    /// </summary>
    private static List<List<int>> ExtractFaceIndices(string block)
    {
        var faces = new List<List<int>>();

        int ciKeyword = block.IndexOf("coordIndex", StringComparison.OrdinalIgnoreCase);
        if (ciKeyword < 0) return faces;

        int arrayStart = block.IndexOf('[', ciKeyword);
        int arrayEnd   = block.IndexOf(']', arrayStart > 0 ? arrayStart : 0);
        if (arrayStart < 0 || arrayEnd < 0) return faces;

        string arrayContent = block[(arrayStart + 1)..arrayEnd];

        var currentFace = new List<int>();
        foreach (var token in Regex.Split(arrayContent, @"[\s,]+"))
        {
            if (token.Length == 0) continue;
            if (!int.TryParse(token, out int idx)) continue;

            if (idx == -1)
            {
                if (currentFace.Count >= 3)
                    faces.Add(new List<int>(currentFace));
                currentFace.Clear();
            }
            else
            {
                currentFace.Add(idx);
            }
        }
        // Handle file that ends without trailing -1
        if (currentFace.Count >= 3)
            faces.Add(currentFace);

        return faces;
    }
}
