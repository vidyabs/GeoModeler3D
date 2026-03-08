namespace GeoModeler3D.Rendering.EntityRenderers;

public class EntityRendererRegistry
{
    private readonly Dictionary<Type, IEntityRenderer> _renderers = new();

    public void Register(IEntityRenderer renderer)
    {
        _renderers[renderer.SupportedEntityType] = renderer;
    }

    public IEntityRenderer GetRenderer(Type entityType)
    {
        if (_renderers.TryGetValue(entityType, out var renderer))
            return renderer;

        throw new InvalidOperationException(
            $"No renderer registered for entity type '{entityType.Name}'.");
    }

    public bool HasRenderer(Type entityType) => _renderers.ContainsKey(entityType);
}
