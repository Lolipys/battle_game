namespace BattleGame.Interfaces;

// Паттерн Command: каждый пункт меню — это команда, которую можно Execute / Undo.
// CommandManager держит стеки истории и redo.
public interface ICommand
{
    string Description { get; }
    void Execute();
    void Undo();
}
