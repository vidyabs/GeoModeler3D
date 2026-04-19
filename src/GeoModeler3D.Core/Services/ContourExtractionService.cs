using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;

namespace GeoModeler3D.Core.Services;

/// <summary>
/// Dispatches contour extraction to the correct algorithm for each entity type.
/// </summary>
public class ContourExtractionService
{
    private const int NSamples = MathConstants.DefaultSegmentCount; // 64

    public IReadOnlyList<ContourCurveEntity> Extract(CuttingPlaneEntity plane, IGeometricEntity entity)
    {
        if (plane.TargetEntityIds.Count == 0) return [];

        var mathPlane = new Plane3D(plane.Origin, plane.Normal);

        return entity switch
        {
            MeshEntity mesh       => ExtractFromMesh(mathPlane, mesh, plane.Id),
            SphereEntity sphere   => ExtractFromSphere(mathPlane, sphere, plane.Id),
            CylinderEntity cyl    => ExtractFromCylinder(mathPlane, cyl, plane.Id),
            ConeEntity cone       => ExtractFromCone(mathPlane, cone, plane.Id),
            TorusEntity torus     => ExtractFromTorus(mathPlane, torus, plane.Id),
            _                    => []
        };
    }

    // ── Mesh (triangle by triangle) ───────────────────────────────────────────

    private static IReadOnlyList<ContourCurveEntity> ExtractFromMesh(
        Plane3D plane, MeshEntity mesh, Guid planeId)
    {
        var segments = PlaneMeshIntersector.Intersect(plane, mesh.Positions);
        if (segments.Count == 0) return [];

        var chains = ContourBuilder.Build(segments);
        var result = new List<ContourCurveEntity>(chains.Count);

        foreach (var (pts, isClosed) in chains)
        {
            var e = new ContourCurveEntity(pts, planeId, mesh.Id) { IsClosed = isClosed };
            result.Add(e);
        }

        return result;
    }

    // ── Sphere (analytic circle) ──────────────────────────────────────────────

    private static IReadOnlyList<ContourCurveEntity> ExtractFromSphere(
        Plane3D plane, SphereEntity sphere, Guid planeId)
    {
        float d = plane.DistanceToPoint(sphere.Center);
        float r = (float)sphere.Radius;
        float r2 = r * r - d * d;
        if (r2 <= 0) return [];

        float circleR = MathF.Sqrt(r2);
        var circleCenter = sphere.Center - d * plane.Normal;
        var pts = SampleCircle(circleCenter, circleR, plane.Normal, NSamples);

        return [new ContourCurveEntity(pts, planeId, sphere.Id) { IsClosed = true }];
    }

    // ── Cylinder (parametric sweep) ───────────────────────────────────────────

    private static IReadOnlyList<ContourCurveEntity> ExtractFromCylinder(
        Plane3D plane, CylinderEntity cyl, Guid planeId)
    {
        float da = Vector3.Dot(cyl.Axis, plane.Normal);
        // Plane nearly parallel to cylinder axis → degenerate
        if (MathF.Abs(da) < 1e-5f) return [];

        var (u, v) = PlaneEntity.ComputeTangents(cyl.Axis);
        float du = Vector3.Dot(u, plane.Normal);
        float dv = Vector3.Dot(v, plane.Normal);
        float d0 = plane.DistanceToPoint(cyl.BaseCenter);
        float r = (float)cyl.Radius;
        float H = (float)cyl.Height;

        var pts = new List<Vector3>(NSamples);
        for (int i = 0; i < NSamples; i++)
        {
            float phi = 2 * MathF.PI * i / NSamples;
            float cos = MathF.Cos(phi);
            float sin = MathF.Sin(phi);

            // h such that plane.dist(P(phi,h)) = 0
            float h = -(d0 + r * cos * du + r * sin * dv) / da;
            if (h < 0 || h > H) continue;

            pts.Add(cyl.BaseCenter + h * cyl.Axis + r * cos * u + r * sin * v);
        }

        if (pts.Count < 2) return [];
        return [new ContourCurveEntity(pts, planeId, cyl.Id) { IsClosed = pts.Count == NSamples }];
    }

    // ── Cone (parametric sweep, varying radius) ───────────────────────────────

    private static IReadOnlyList<ContourCurveEntity> ExtractFromCone(
        Plane3D plane, ConeEntity cone, Guid planeId)
    {
        var (u, v) = PlaneEntity.ComputeTangents(cone.Axis);
        float da = Vector3.Dot(cone.Axis, plane.Normal);
        float du = Vector3.Dot(u, plane.Normal);
        float dv = Vector3.Dot(v, plane.Normal);
        float d0 = plane.DistanceToPoint(cone.BaseCenter);
        float R = (float)cone.BaseRadius;
        float H = (float)cone.Height;

        var pts = new List<Vector3>(NSamples);
        for (int i = 0; i < NSamples; i++)
        {
            float phi = 2 * MathF.PI * i / NSamples;
            float cos = MathF.Cos(phi);
            float sin = MathF.Sin(phi);

            // P(phi,h) = BaseCenter + h*Axis + R*(1-h/H)*(cos*u + sin*v)
            // dist = d0 + h*da + R*(1-h/H)*(cos*du + sin*dv) = 0
            float dPhi = R * (cos * du + sin * dv);
            float denom = da - dPhi / H;
            if (MathF.Abs(denom) < 1e-7f) continue;

            float h = -(d0 + dPhi) / denom;
            if (h < 0 || h > H) continue;

            float rH = R * (1 - h / H);
            pts.Add(cone.BaseCenter + h * cone.Axis + rH * cos * u + rH * sin * v);
        }

        if (pts.Count < 2) return [];

        var conicType = ConicSectionClassifier.Classify(plane, cone);
        return [new ContourCurveEntity(pts, planeId, cone.Id)
        {
            IsClosed = pts.Count == NSamples,
            ConicType = conicType
        }];
    }

    // ── Torus (analytic per ring) ─────────────────────────────────────────────

    private static IReadOnlyList<ContourCurveEntity> ExtractFromTorus(
        Plane3D plane, TorusEntity torus, Guid planeId)
    {
        var (u, v) = PlaneEntity.ComputeTangents(torus.Normal);
        float R = (float)torus.MajorRadius;
        float r = (float)torus.MinorRadius;
        float dN = Vector3.Dot(torus.Normal, plane.Normal);

        // Each major angle phi produces a tube ring.
        // Ring center: C(phi) = torus.Center + R*(cos*u + sin*v)
        // Ring points: C(phi) + r*(cos(theta)*radial + sin(theta)*Normal)
        // Plane condition: dc + A*cos(theta) + B*sin(theta) = 0
        // where A = r*dot(radial, planeNormal), B = r*dN
        // Two solutions per ring → two closed curves

        int nPhi = 128;
        var curve1 = new List<Vector3>(nPhi);
        var curve2 = new List<Vector3>(nPhi);

        for (int i = 0; i < nPhi; i++)
        {
            float phi = 2 * MathF.PI * i / nPhi;
            var radial = MathF.Cos(phi) * u + MathF.Sin(phi) * v;
            var ringCenter = torus.Center + R * radial;

            float dc = plane.DistanceToPoint(ringCenter);
            float A = r * Vector3.Dot(radial, plane.Normal);
            float B = r * dN;
            float amp = MathF.Sqrt(A * A + B * B);

            if (amp < 1e-10f || MathF.Abs(dc) > amp + 1e-6f) continue;

            float ratio = System.Math.Clamp(-dc / amp, -1f, 1f);
            float phiBase = MathF.Atan2(B, A);
            float dTheta = MathF.Acos(ratio);

            float theta1 = phiBase + dTheta;
            float theta2 = phiBase - dTheta;

            curve1.Add(ringCenter + r * (MathF.Cos(theta1) * radial + MathF.Sin(theta1) * torus.Normal));
            curve2.Add(ringCenter + r * (MathF.Cos(theta2) * radial + MathF.Sin(theta2) * torus.Normal));
        }

        var result = new List<ContourCurveEntity>(2);
        if (curve1.Count >= 2)
            result.Add(new ContourCurveEntity(curve1, planeId, torus.Id)
                { IsClosed = curve1.Count == nPhi });

        if (curve2.Count >= 2 && AreDistinctCurves(curve1, curve2))
            result.Add(new ContourCurveEntity(curve2, planeId, torus.Id)
                { IsClosed = curve2.Count == nPhi });

        return result;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<Vector3> SampleCircle(Vector3 center, float radius, Vector3 normal, int n)
    {
        var (u, v) = PlaneEntity.ComputeTangents(normal);
        var pts = new List<Vector3>(n);
        for (int i = 0; i < n; i++)
        {
            float phi = 2 * MathF.PI * i / n;
            pts.Add(center + radius * (MathF.Cos(phi) * u + MathF.Sin(phi) * v));
        }
        return pts;
    }

    private static bool AreDistinctCurves(List<Vector3> c1, List<Vector3> c2)
    {
        if (c1.Count == 0 || c2.Count == 0 || c1.Count != c2.Count) return true;
        float sumDist = 0;
        for (int i = 0; i < c1.Count; i++)
            sumDist += (c1[i] - c2[i]).Length();
        return sumDist / c1.Count > 0.01f;
    }
}
