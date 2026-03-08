using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Rendering.Extensions;

public static class ConversionExtensions
{
    // Vector3 -> WPF 3D types
    public static Point3D ToPoint3D(this Vector3 v) => new(v.X, v.Y, v.Z);
    public static Vector3D ToVector3D(this Vector3 v) => new(v.X, v.Y, v.Z);

    // WPF 3D types -> Vector3
    public static Vector3 ToVector3(this Point3D p) => new((float)p.X, (float)p.Y, (float)p.Z);
    public static Vector3 ToVector3(this Vector3D v) => new((float)v.X, (float)v.Y, (float)v.Z);

    // EntityColor <-> WPF Color
    public static Color ToWpfColor(this EntityColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);
    public static EntityColor ToEntityColor(this Color c) => new(c.R, c.G, c.B, c.A);
}
