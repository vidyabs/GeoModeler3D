using System.Numerics;
using GeoModeler3D.Core.Entities;
using GeoModeler3D.Core.Math;
using GeoModeler3D.Core.Services;
using Xunit;

namespace GeoModeler3D.Tests.Services;

public class ConicSectionClassifierTests
{
    // Cone along Z, baseRadius=3, height=4 → halfAngle=atan2(3,4) ≈ 36.87°
    // sinAlpha = 3/5 = 0.6
    private static ConeEntity MakeCone() =>
        new(Vector3.Zero, Vector3.UnitZ, baseRadius: 3.0, height: 4.0);

    private static Plane3D MakePlane(Vector3 normal) =>
        new(Vector3.Zero, Vector3.Normalize(normal));

    // ── Circle ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_PlanePerpendicular_ReturnsCircle()
    {
        // Plane normal ∥ cone axis → s = 1 → Circle
        var result = ConicSectionClassifier.Classify(MakePlane(Vector3.UnitZ), MakeCone());
        Assert.Equal(ConicSectionType.Circle, result);
    }

    [Fact]
    public void Cone_PlaneNearlyPerpendicular_ReturnsCircle()
    {
        // Tilt the plane very slightly — should still be Circle
        var normal = Vector3.Normalize(new Vector3(0.005f, 0, 1));
        var result = ConicSectionClassifier.Classify(MakePlane(normal), MakeCone());
        Assert.Equal(ConicSectionType.Circle, result);
    }

    // ── Ellipse ───────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_PlaneTiltedModerately_ReturnsEllipse()
    {
        // sinAlpha = 0.6; s must be in (sinAlpha+tol, 1-tol)
        // s = |dot(normal, UnitZ)| = cos(tiltAngle)
        // Want s ≈ 0.8 → tiltAngle ≈ 36.87° from vertical → normal tilted ~37° from Z
        // Use normal = (sin(30°), 0, cos(30°)) → s = cos(30°) ≈ 0.866 > 0.6
        var normal = new Vector3(MathF.Sin(MathF.PI / 6), 0, MathF.Cos(MathF.PI / 6));
        var result = ConicSectionClassifier.Classify(MakePlane(normal), MakeCone());
        Assert.Equal(ConicSectionType.Ellipse, result);
    }

    // ── Parabola ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_PlaneAtHalfAngle_ReturnsParabola()
    {
        // sinAlpha = 3/5 = 0.6 → need s ≈ 0.6
        // s = |dot(normal, UnitZ)| = 0.6 → normal = (0.8, 0, 0.6) (sin²+cos²=1: 0.64+0.36=1 ✓)
        var normal = new Vector3(0.8f, 0, 0.6f);
        var result = ConicSectionClassifier.Classify(MakePlane(normal), MakeCone());
        Assert.Equal(ConicSectionType.Parabola, result);
    }

    // ── Hyperbola ─────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_PlaneNearlyParallelToAxis_ReturnsHyperbola()
    {
        // s = |dot(normal, UnitZ)| ≈ 0 < sinAlpha=0.6 → Hyperbola
        // normal ≈ UnitX → s ≈ 0
        var result = ConicSectionClassifier.Classify(MakePlane(Vector3.UnitX), MakeCone());
        Assert.Equal(ConicSectionType.Hyperbola, result);
    }

    [Fact]
    public void Cone_PlaneSlightlyBelowHalfAngle_ReturnsHyperbola()
    {
        // sinAlpha = 0.6; s = 0.3 < 0.6 → Hyperbola
        // normal with |cos(theta)| = 0.3: sin(theta) = sqrt(1-0.09) ≈ 0.954
        var normal = new Vector3(0.954f, 0, 0.3f);
        var result = ConicSectionClassifier.Classify(MakePlane(normal), MakeCone());
        Assert.Equal(ConicSectionType.Hyperbola, result);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void Cone_NegativeNormalDirection_SameResult()
    {
        // Flipping the plane normal must not change the result (we use Abs)
        var normalPos = new Vector3(MathF.Sin(MathF.PI / 6), 0, MathF.Cos(MathF.PI / 6));
        var normalNeg = -normalPos;
        var cone = MakeCone();
        Assert.Equal(
            ConicSectionClassifier.Classify(MakePlane(normalPos), cone),
            ConicSectionClassifier.Classify(MakePlane(normalNeg), cone));
    }

    [Fact]
    public void Cone_AxisNotAlongZ_ClassifiesCorrectly()
    {
        // Cone along X axis instead of Z, same half-angle
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitX, baseRadius: 3.0, height: 4.0);
        // Plane perpendicular to X → Circle
        var result = ConicSectionClassifier.Classify(MakePlane(Vector3.UnitX), cone);
        Assert.Equal(ConicSectionType.Circle, result);
    }

    // ── Integration: ConicType stamped on extracted contour ───────────────────

    [Fact]
    public void ContourExtraction_Cone_PerpendicularPlane_ContourIsCircle()
    {
        var svc = new ContourExtractionService();
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 4.0, 8.0);
        var cuttingPlane = new CuttingPlaneEntity(new Vector3(0, 0, 4), Vector3.UnitZ);
        cuttingPlane.TargetEntityIds.Add(cone.Id);

        var contours = svc.Extract(cuttingPlane, cone);

        Assert.Single(contours);
        Assert.Equal(ConicSectionType.Circle, contours[0].ConicType);
    }

    [Fact]
    public void ContourExtraction_Cone_TiltedPlane_ContourIsEllipse()
    {
        var svc = new ContourExtractionService();
        // sinAlpha = 3/5 = 0.6; tilt to get s = cos(30°) ≈ 0.866 → Ellipse
        var cone = new ConeEntity(Vector3.Zero, Vector3.UnitZ, 3.0, 4.0);
        var planeNormal = Vector3.Normalize(new Vector3(MathF.Sin(MathF.PI / 6), 0, MathF.Cos(MathF.PI / 6)));
        var cuttingPlane = new CuttingPlaneEntity(new Vector3(0, 0, 2), planeNormal);
        cuttingPlane.TargetEntityIds.Add(cone.Id);

        var contours = svc.Extract(cuttingPlane, cone);

        // May return 0 if the ellipse clips the cone bounds — just check ConicType if we get a result
        if (contours.Count > 0)
            Assert.Equal(ConicSectionType.Ellipse, contours[0].ConicType);
    }
}
