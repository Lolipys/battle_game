using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services;

// Паттерн Proxy для логгера: оборачивает IBattleLogger и добавляет звуковые эффекты.
// Все методы делегируются реальному логгеру, но при смерти юнита издаётся звук.
// Демонстрирует Proxy: тот же интерфейс, но с дополнительным поведением.
public class SoundLoggerProxy : IBattleLogger
{
    private readonly IBattleLogger _inner;

    public SoundLoggerProxy(IBattleLogger inner)
    {
        _inner = inner;
    }

    public void TurnHeader(int turn) => _inner.TurnHeader(turn);
    public void PhaseHeader(string phase) => _inner.PhaseHeader(phase);
    public void MeleeAttack(Unit attacker, Unit defender, int damage, int hpBefore) => _inner.MeleeAttack(attacker, defender, damage, hpBefore);
    public void CounterAttack(Unit defender, Unit attacker, int damage, int hpBefore) => _inner.CounterAttack(defender, attacker, damage, hpBefore);
    public void AbilityUsed(Unit user, Unit target, int damage, int position, int range, int hpBefore) => _inner.AbilityUsed(user, target, damage, position, range, hpBefore);
    public void AbilityOutOfRange(Unit user, int position, int range) => _inner.AbilityOutOfRange(user, position, range);
    public void CleanupResult(int dead1, int dead2, Army army1, Army army2) => _inner.CleanupResult(dead1, dead2, army1, army2);
    public void HealUsed(Unit healer, Unit target, int healAmount, int position, int range) => _inner.HealUsed(healer, target, healAmount, position, range);
    public void CloneUsed(Unit wizard, Unit original, Unit clone, int position) => _inner.CloneUsed(wizard, original, clone, position);
    public void CloneFailed(Unit wizard, int position) => _inner.CloneFailed(wizard, position);
    public void ShieldAbsorbed(Unit target, int absorbed, int shieldRemaining) => _inner.ShieldAbsorbed(target, absorbed, shieldRemaining);
    public void ShieldDestroyed(Unit target) => _inner.ShieldDestroyed(target);
    public void GameOver(Army? winner) => _inner.GameOver(winner);
    public void PrintArmyStatus(Army army1, Army army2) => _inner.PrintArmyStatus(army1, army2);

    // Proxy-поведение: при смерти юнита — звуковой сигнал + делегирование реальному логгеру
    public void UnitDied(Unit unit, string armyName)
    {
        _inner.UnitDied(unit, armyName);

        // Звуковой сигнал при гибели юнита
        try { Console.Beep(300, 150); }
        catch { /* Beep не поддерживается на некоторых платформах */ }
    }
}
