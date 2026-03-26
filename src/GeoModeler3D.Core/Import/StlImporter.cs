using System.Globalization;
using System.Numerics;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Import;

/// <summary>Imports triangle meshes from STL files (ASCII and binary).</summary>
public class StlImporter : IFileImporter
{
    public string FormatName => "STL";
    public string FileFilter => "STL Files (*.stl)|*.stl";

    public ImportValidationResult Validate(string filePath)
    {
        if (!File.Exists(filePath))
            return new ImportValidationResult(false, "File not found.");

        try
        {
            long fileSize = new FileInfo(filePath).Length;
            if (fileSize < 84)
                return new ImportValidationResult(false, "File is too small to be a valid STL.");

            if (IsAscii(filePath))
            {
                string text = File.ReadAllText(filePath);
                int count = CountOccurrences(text, "facet normal");
                return new ImportValidationResult(true, null, count, fileSize);
            }
            else
            {
                using var fs = File.OpenRead(filePath);
                fs.Seek(80, SeekOrigin.Begin);
                using var br = new BinaryReader(fs);
                uint count = br.ReadUInt32();
                long expected = 84 + count * 50L;
                if (fileSize != expected)
                    return new ImportValidationResult(false,
                        $"Binary STL size mismatch: expected {expected} bytes, got {fileSize}.");
                return new ImportValidationResult(true, null, (int)count, fileSize);
            }
        }
        catch (Exception ex)
        {
            return new ImportValidationResult(false, ex.Message);
        }
    }

    public IReadOnlyList<IGeometricEntity> Import(string filePath)
    {
        return IsAscii(filePath) ? ImportAscii(filePath) : ImportBinary(filePath);
    }

    // ---------------------------------------------------------------

    private static bool IsAscii(string filePath)
    {
        // Read first 256 bytes and look for "solid" keyword, then rule out
        // binary files that coincidentally start with "solid".
        try
        {
            using var fs = File.OpenRead(filePath);
            Span<byte> header = stackalloc byte[256];
            int read = fs.Read(header);
            string headerText = System.Text.Encoding.ASCII.GetString(header[..read]);
            bool hasKeyword = headerText.TrimStart().StartsWith("solid", StringComparison.OrdinalIgnoreCase);
            if (!hasKeyword) return false;

            long fileSize = new FileInfo(filePath).Length;
            if (fileSize < 84) return true;
            fs.Seek(80, SeekOrigin.Begin);
            using var br = new BinaryReader(fs);
            uint binaryCount = br.ReadUInt32();
            long expected = 84 + binaryCount * 50L;
            if (fileSize == expected && binaryCount > 0) return false; // binary

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<IGeometricEntity> ImportAscii(string filePath)
    {
        var result = new List<IGeometricEntity>();
        var verts = new Vector3[3];
        int vertIdx = 0;

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("vertex ", StringComparison.OrdinalIgnoreCase))
            {
                verts[vertIdx++] = ParseVector(line["vertex ".Length..]);
                if (vertIdx == 3)
                {
                    result.Add(new TriangleEntity(verts[0], verts[1], verts[2]));
                    vertIdx = 0;
                }
            }
        }
        return result;
    }

    private static List<IGeometricEntity> ImportBinary(string filePath)
    {
        var result = new List<IGeometricEntity>();
        using var fs = File.OpenRead(filePath);
        using var br = new BinaryReader(fs);

        br.ReadBytes(80); // skip header
        uint count = br.ReadUInt32();
        for (uint i = 0; i < count; i++)
        {
            br.ReadBytes(12); // skip normal (3 × float32)
            var v0 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            var v1 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            var v2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            br.ReadUInt16(); // attribute byte count
            result.Add(new TriangleEntity(v0, v1, v2));
        }
        return result;
    }

    private static Vector3 ParseVector(string s)
    {
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return new Vector3(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture));
    }

    private static int CountOccurrences(string text, string keyword)
    {
        int count = 0, idx = 0;
        while ((idx = text.IndexOf(keyword, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += keyword.Length;
        }
        return count;
    }
}
