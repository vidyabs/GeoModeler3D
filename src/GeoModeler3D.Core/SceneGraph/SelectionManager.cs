namespace GeoModeler3D.Core.SceneGraph;

public class SelectionManager
{
    private readonly List<Guid> _selectedIds = [];

    public IReadOnlyList<Guid> SelectedIds => _selectedIds.AsReadOnly();
    public event Action? SelectionChanged;

    public void Select(Guid id)
    {
        _selectedIds.Clear();
        _selectedIds.Add(id);
        SelectionChanged?.Invoke();
    }

    public void ToggleSelect(Guid id)
    {
        if (_selectedIds.Contains(id))
            _selectedIds.Remove(id);
        else
            _selectedIds.Add(id);
        SelectionChanged?.Invoke();
    }

    public void AddToSelection(Guid id)
    {
        if (!_selectedIds.Contains(id))
        {
            _selectedIds.Add(id);
            SelectionChanged?.Invoke();
        }
    }

    public void ClearSelection()
    {
        if (_selectedIds.Count == 0) return;
        _selectedIds.Clear();
        SelectionChanged?.Invoke();
    }

    public bool IsSelected(Guid id) => _selectedIds.Contains(id);
}
