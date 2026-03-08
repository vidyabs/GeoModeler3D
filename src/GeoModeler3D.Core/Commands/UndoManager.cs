namespace GeoModeler3D.Core.Commands;

public class UndoManager
{
    private readonly Stack<IUndoableCommand> _undoStack = new();
    private readonly Stack<IUndoableCommand> _redoStack = new();

    public int MaxDepth { get; set; } = 50;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public string? UndoDescription => CanUndo ? _undoStack.Peek().Description : null;
    public string? RedoDescription => CanRedo ? _redoStack.Peek().Description : null;

    public event Action? StackChanged;

    public void Execute(IUndoableCommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();

        // Trim stack if it exceeds max depth
        if (_undoStack.Count > MaxDepth)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = items.Length - MaxDepth; i < items.Length; i++)
                _undoStack.Push(items[i]);
            // Reverse because Stack enumerates LIFO
            var trimmed = _undoStack.ToArray();
            _undoStack.Clear();
            foreach (var item in trimmed)
                _undoStack.Push(item);
        }

        StackChanged?.Invoke();
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        StackChanged?.Invoke();
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        StackChanged?.Invoke();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        StackChanged?.Invoke();
    }
}
