using GeoModeler3D.Core.Entities;

namespace GeoModeler3D.Core.Serialization;

/// <summary>Creates serializers for specific entity types.</summary>
public interface IEntitySerializerFactory
{
    bool CanSerialize(Type entityType);
    string Serialize(IGeometricEntity entity);
    IGeometricEntity Deserialize(string data, Type entityType);
}
