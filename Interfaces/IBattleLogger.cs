using BattleGame.Units;

namespace BattleGame.Interfaces;

// Контракт логирования боевых событий.
// Принцип D (SOLID): Battlefield зависит от этого интерфейса, а не от конкретного BattleLog.
// Позволяет подменить вывод (консоль, файл, тесты) без изменения логики боя.
public interface IBattleLogger
{
    void TurnHeader(int turn);
    void PhaseHeader(string phase);
    void MeleeAttack(Unit attacker, Unit defender, int damage, int hpBefore);
    void CounterAttack(Unit defender, Unit attacker, int damage, int hpBefore);
    void AbilityUsed(Unit user, Unit target, int damage, int position, int range, int hpBefore);
    void AbilityOutOfRange(Unit user, int position, int range);
    void UnitDied(Unit unit, string armyName);
    void CleanupResult(int dead1, int dead2, Army army1, Army army2);
    void HealUsed(Unit healer, Unit target, int healAmount, int position, int range);
    void CloneUsed(Unit wizard, Unit original, Unit clone, int position);
    void CloneFailed(Unit wizard, int position);
    void ShieldAbsorbed(Unit target, int absorbed, int shieldRemaining);
    void ShieldDestroyed(Unit target);
    void GameOver(Army? winner);
    void PrintArmyStatus(Army army1, Army army2);
}
