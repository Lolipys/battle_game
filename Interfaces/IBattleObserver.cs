using BattleGame.Units;

namespace BattleGame.Interfaces;

// Observer (GoF, без C# event): DeathObserver и DamageObserver подписываются на события боя
public interface IBattleObserver
{
    void OnDamage(Unit target, Army army, int damage, int hpBefore);
    void OnDeath(Unit unit, Army army);
    // используется DamageObserver для подсчёта ходов без урона
    void OnTurnStarted(int turnNumber);
}
