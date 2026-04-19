using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;
using GeoModeler3D.Rendering.Extensions;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

/// <summary>
/// Generates a filled cap (polygon) for a cutting-plane cross-section.
/// Accepts a closed <see cref="ContourCurveEntity"/> and returns a double-sided
/// <see cref="ModelVisual3D"/> whose geometry lies coplanar with the cutting plane.
/// </summary>
public static class CappingVisualGenerator
{
    /// <summary>
    /// Generates a cap visual for the given contour, or returns <c>null</c> if
    /// the contour is open, has fewer than 3 points, or triangulation fails.
    /// </summary>
    /// <param name="contour">Closed contour to cap.</param>
    /// <param name="planeNormal">Normal of the cutting plane (for projection).</param>
    /// <param name="planeOrigin">A point on the cutting plane.</param>
    /// <param name="color">Fill colour for the cap face.</param>
    public static ModelVisual3D? Generate(
        ContourCurveEntity contour,
        Vector3 planeNormal,
        Vector3 planeOrigin,
        Color color)
    {
        if (!contour.IsClosed || contour.Points.Count < 3)
            return null;

        // Project contour to 2-D for triangulation
        var (points2D, _, _) = PlaneProjector.Project(contour.Points, planeNormal, planeOrigin);

        var triangleIndices = EarClippingTriangulator.Triangulate(points2D);
        if (triangleIndices.Count < 3)
            return null;

        // Build WPF MeshGeometry3D from the original 3-D points
        var mesh = new MeshGeometry3D();
        var normal3D = planeNormal.ToVector3D();

        foreach (var p in contour.Points)
        {
            mesh.Positions.Add(p.ToPoint3D());
            mesh.Normals.Add(normal3D);
        }

        foreach (var idx in triangleIndices)
            mesh.TriangleIndices.Add(idx);

        var material = MaterialHelper.CreateMaterial(color);
        var geo = new GeometryModel3D
        {
            Geometry = mesh,
            Material = material,
            BackMaterial = material   // double-sided: visible from both sides
        };

        return new ModelVisual3D { Content = geo };
    }
}
