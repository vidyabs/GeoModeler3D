namespace GeoModeler3D.Core.Commands;

public class MacroCommand : IUndoableCommand
{
    private readonly List<IUndoableCommand> _children;

    public MacroCommand(string description, IEnumerable<IUndoableCommand> children)
    {
        Description = description;
        _children = children.ToList();
    }

    public MacroCommand(string description) : this(description, [])
    {
    }

    public string Description { get; }

    public void Add(IUndoableCommand command) => _children.Add(command);

    public void Execute()
    {
        foreach (var child in _children)
            child.Execute();
    }

    public void Undo()
    {
        for (int i = _children.Count - 1; i >= 0; i--)
            _children[i].Undo();
    }
}
