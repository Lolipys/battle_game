namespace BattleGame.Interfaces;

// Command: каждое действие игрока — Execute / Undo; стеки управляет CommandManager
public interface ICommand
{
    string Description { get; }
    void Execute();
    void Undo();
}
