using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace GeoModeler3D.Rendering;

public class SelectionHighlighter
{
    private readonly Dictionary<Guid, Material?> _originalMaterials = new();

    private static readonly Material HighlightMaterial =
        MaterialHelper.CreateMaterial(Color.FromArgb(180, 255, 255, 0));

    public void Highlight(Guid entityId, Visual3D visual)
    {
        if (_originalMaterials.ContainsKey(entityId)) return;

        if (visual is MeshElement3D meshElement)
        {
            _originalMaterials[entityId] = meshElement.Material;
            meshElement.Material = HighlightMaterial;
        }
        else if (visual is ModelVisual3D mv && mv.Content is GeometryModel3D gmContent)
        {
            _originalMaterials[entityId] = gmContent.Material;
            gmContent.Material = HighlightMaterial;
        }
    }

    public void RemoveHighlight(Guid entityId, Visual3D visual)
    {
        if (!_originalMaterials.TryGetValue(entityId, out var originalMaterial))
            return;

        if (visual is MeshElement3D meshElement)
            meshElement.Material = originalMaterial;
        else if (visual is ModelVisual3D mv && mv.Content is GeometryModel3D gmContent)
            gmContent.Material = originalMaterial;

        _originalMaterials.Remove(entityId);
    }

    public void ClearAll(Dictionary<Guid, Visual3D> visuals)
    {
        foreach (var kvp in _originalMaterials.ToList())
        {
            if (visuals.TryGetValue(kvp.Key, out var visual))
                RemoveHighlight(kvp.Key, visual);
        }
        _originalMaterials.Clear();
    }
}
