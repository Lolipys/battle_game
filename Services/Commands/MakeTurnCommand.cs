using BattleGame.Interfaces;
using BattleGame.Services.Observers;
using BattleGame.Services.Strategies;

namespace BattleGame.Services.Commands;

// Команда "сделать ход": сохраняет состояние, выполняет ход, при Undo откатывает.
public class MakeTurnCommand : ICommand
{
    private readonly Battlefield _bf;
    private readonly DamageObserver? _damageObs;
    private string _snapshot = string.Empty;
    private int _staticTurnsBefore;

    public string Description => "Сделать ход";

    public MakeTurnCommand(Battlefield bf, DamageObserver? damageObs)
    {
        _bf = bf;
        _damageObs = damageObs;
    }

    public void Execute()
    {
        _snapshot = _bf.Snapshot();
        _staticTurnsBefore = _damageObs?.StaticTurns ?? 0;
        _bf.MakeTurn();
    }

    public void Undo()
    {
        _bf.Restore(_snapshot);
        _bf.SetStrategy(StrategyResolver.Resolve(_bf.StrategyName));
        _damageObs?.Reset();
    }
}
