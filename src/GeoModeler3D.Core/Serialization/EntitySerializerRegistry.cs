namespace GeoModeler3D.Core.Serialization;

/// <summary>Registry of entity serializers, keyed by entity type name.</summary>
public class EntitySerializerRegistry
{
    private readonly Dictionary<string, IEntitySerializerFactory> _factories = new();

    public void Register(string typeName, IEntitySerializerFactory factory)
    {
        _factories[typeName] = factory;
    }

    public IEntitySerializerFactory? GetFactory(string typeName)
    {
        return _factories.GetValueOrDefault(typeName);
    }
}
