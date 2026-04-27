using BattleGame.Interfaces;
using BattleGame.Services.Observers;
using BattleGame.Services.Strategies;

namespace BattleGame.Services.Commands;

// Команда "играть до конца" — снимок один в начале, Undo откатывает к этому снимку.
public class PlayToEndCommand : ICommand
{
    private readonly Battlefield _bf;
    private readonly DamageObserver? _damageObs;
    private string _snapshot = string.Empty;

    public string Description => "Играть до конца";

    public PlayToEndCommand(Battlefield bf, DamageObserver? damageObs)
    {
        _bf = bf;
        _damageObs = damageObs;
    }

    public void Execute()
    {
        _snapshot = _bf.Snapshot();
        _bf.PlayToEnd();
    }

    public void Undo()
    {
        _bf.Restore(_snapshot);
        _bf.SetStrategy(StrategyResolver.Resolve(_bf.StrategyName));
        _damageObs?.Reset();
    }
}
