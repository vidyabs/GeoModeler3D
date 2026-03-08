using System.Numerics;

namespace GeoModeler3D.Core.Operations;

/// <summary>Provides measurement utilities: distance, angle, area.</summary>
public class MeasurementService
{
    public double PointToPointDistance(Vector3 a, Vector3 b) => Vector3.Distance(a, b);

    public double AngleBetween(Vector3 a, Vector3 b)
    {
        // TODO: implement angle measurement
        return 0;
    }

    public double SurfaceArea(Guid entityId)
    {
        // TODO: compute surface area for supported entities
        return 0;
    }
}
