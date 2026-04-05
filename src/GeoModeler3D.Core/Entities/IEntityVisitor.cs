namespace GeoModeler3D.Core.Entities;

public interface IEntityVisitor
{
    void Visit(PointEntity entity);
    void Visit(TriangleEntity entity);
    void Visit(CircleEntity entity);
    void Visit(SphereEntity entity);
    void Visit(CylinderEntity entity);
    void Visit(ConeEntity entity);
    void Visit(TorusEntity entity);
    void Visit(MeshEntity entity);
    void Visit(VectorEntity entity);
    void Visit(PlaneEntity entity);
    void Visit(CuttingPlaneEntity entity);
    void Visit(ContourCurveEntity entity);
}
