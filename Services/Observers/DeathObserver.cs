using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Observers;

// заменяет SoundLoggerProxy — вместо Proxy-обёртки логгера теперь отдельная подписка
public class DeathObserver : IBattleObserver
{
    private readonly IBattleLogger _logger;
    private readonly Dictionary<string, int> _deathCount = new();

    public DeathObserver(IBattleLogger logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<string, int> DeathCount => _deathCount;

    public void OnDeath(Unit unit, Army army)
    {
        _logger.UnitDied(unit, army.Name);

        if (_deathCount.ContainsKey(army.Name))
            _deathCount[army.Name]++;
        else
            _deathCount[army.Name] = 1;
    }

    // пустая реализация: урон отслеживает DamageObserver
    public void OnDamage(Unit target, Army army, int damage, int hpBefore) { }

    public void OnTurnStarted(int turnNumber) { }
}
