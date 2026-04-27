using BattleGame.Interfaces;
using BattleGame.Services.Strategies;

namespace BattleGame.Services.Commands;

// Команда "сменить построение".
public class ChangeStrategyCommand : ICommand
{
    private readonly Battlefield _bf;
    private readonly IBattleStrategy _newStrategy;
    private IBattleStrategy _oldStrategy = new BridgeStrategy();

    public string Description => $"Перепостроение: {_newStrategy.Name}";

    public ChangeStrategyCommand(Battlefield bf, IBattleStrategy newStrategy)
    {
        _bf = bf;
        _newStrategy = newStrategy;
    }

    public void Execute()
    {
        _oldStrategy = _bf.Strategy;
        _bf.SetStrategy(_newStrategy);
    }

    public void Undo()
    {
        _bf.SetStrategy(_oldStrategy);
    }
}
