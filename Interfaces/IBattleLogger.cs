using BattleGame.Units;

namespace BattleGame.Interfaces;

public interface IBattleLogger
{
    void TurnHeader(int turn);
    void PhaseHeader(string phase);
    void MeleeAttack(Unit attacker, Unit defender, int damage);
    void CounterAttack(Unit defender, Unit attacker, int damage);
    void AbilityUsed(Unit user, Unit target, int damage, int position, int range);
    void AbilityOutOfRange(Unit user, int position, int range);
    void UnitDied(Unit unit, string armyName);
    void CleanupResult(int dead1, int dead2, Army army1, Army army2);
    void GameOver(Army? winner);
    void PrintArmyStatus(Army army1, Army army2);
}