using BattleGame.Interfaces;

namespace BattleGame.Services.Commands;

// Reset — откат до начального состояния: последовательный Undo до пустой истории
public class CommandManager
{
    private readonly Stack<ICommand> _history = new();
    private readonly Stack<ICommand> _redo = new();

    public bool CanUndo => _history.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public int HistoryDepth => _history.Count;

    public void Execute(ICommand command)
    {
        command.Execute();
        _history.Push(command);
        _redo.Clear();
    }

    public bool Undo()
    {
        if (!CanUndo) return false;
        var cmd = _history.Pop();
        cmd.Undo();
        _redo.Push(cmd);
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo) return false;
        var cmd = _redo.Pop();
        cmd.Execute();
        _history.Push(cmd);
        return true;
    }

    public void Reset()
    {
        while (CanUndo) Undo();
    }
}
