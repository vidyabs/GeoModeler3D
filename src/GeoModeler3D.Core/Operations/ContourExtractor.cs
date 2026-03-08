using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Operations;

/// <summary>Extracts contour curves from entities intersected by a cutting plane.</summary>
public class ContourExtractor
{
    public ContourCurveEntity? ExtractContour(IGeometricEntity entity, Plane3D cuttingPlane)
    {
        // TODO: implement contour extraction for spheres, cylinders, cones, torus
        return null;
    }
}
