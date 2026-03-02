using System.Text.Json.Serialization;
using BattleGame.Interfaces;
using BattleGame.Services;
using BattleGame.Units;

namespace BattleGame;

public class Battlefield
{
    public Army Army1 { get; set; } = new();
    public Army Army2 { get; set; } = new();
    public int TurnNumber { get; set; }

    [JsonIgnore]
    public IBattleLogger Logger { get; set; } = new BattleLog();

    [JsonIgnore]
    public bool IsGameOver => Army1.IsDefeated || Army2.IsDefeated;

    public Battlefield() { }

    public Battlefield(IBattleLogger logger)
    {
        Logger = logger;
    }

    public void MakeTurn()
    {
        if (IsGameOver)
        {
            Logger.GameOver(GetWinner());
            return;
        }

        TurnNumber++;
        Logger.TurnHeader(TurnNumber);

        MeleePhase();
        SpecialAbilityPhase();
        CleanupPhase();

        Logger.PrintArmyStatus(Army1, Army2);

        if (IsGameOver)
            Logger.GameOver(GetWinner());
    }

    public void PlayToEnd()
    {
        while (!IsGameOver)
        {
            MakeTurn();
        }
    }

    public void PrintArmyStatus()
    {
        Logger.PrintArmyStatus(Army1, Army2);
    }

    private void MeleePhase()
    {
        Logger.PhaseHeader("Ближний бой");

        var attacker = Army1.FirstUnit;
        var defender = Army2.FirstUnit;

        if (attacker == null || defender == null)
            return;

        int damage = attacker.Attack(defender);
        Logger.MeleeAttack(attacker, defender, damage);

        if (!defender.IsAlive)
        {
            Logger.UnitDied(defender, Army2.Name);
            return;
        }

        int counterDamage = defender.Attack(attacker);
        Logger.CounterAttack(defender, attacker, counterDamage);

        if (!attacker.IsAlive)
            Logger.UnitDied(attacker, Army1.Name);
    }

    private void SpecialAbilityPhase()
    {
        bool hasAbilities = HasSpecialAbilityUnits(Army1) || HasSpecialAbilityUnits(Army2);
        if (!hasAbilities)
            return;

Logger.PhaseHeader("Специальные способности");

        ProcessArmyAbilities(Army1, Army2);
        ProcessArmyAbilities(Army2, Army1);
    }

    private static bool HasSpecialAbilityUnits(Army army)
    {
        for (int i = 1; i < army.Units.Count; i++)
        {
            if (army.Units[i] is ISpecialAbility)
                return true;
        }
        return false;
    }

    private void ProcessArmyAbilities(Army allies, Army enemies)
    {
        for (int i = 1; i < allies.Units.Count; i++)
        {
            if (!allies.Units[i].IsAlive)
                continue;

            if (allies.Units[i] is not ISpecialAbility ability)
                continue;

            var target = enemies.FirstAliveUnit;
            if (target == null)
                break;

            int position = i + 1;

            if (ability.Range < position)
            {
                Logger.AbilityOutOfRange(allies.Units[i], position, ability.Range);
                continue;
            }

            int damage = ability.UseAbility(target);
            Logger.AbilityUsed(allies.Units[i], target, damage, position, ability.Range);

            if (!target.IsAlive)
                Logger.UnitDied(target, enemies.Name);
        }
    }

    private void CleanupPhase()
    {
        int deadArmy1 = Army1.Units.Count(u => !u.IsAlive);
        int deadArmy2 = Army2.Units.Count(u => !u.IsAlive);

        Army1.RemoveDeadUnits();
        Army2.RemoveDeadUnits();

        if (deadArmy1 + deadArmy2 > 0)
        {
            Logger.PhaseHeader("Очистка поля");
            Logger.CleanupResult(deadArmy1, deadArmy2, Army1, Army2);
        }
    }

    private Army? GetWinner()
    {
        if (Army1.IsDefeated && Army2.IsDefeated)
            return null;
        if (Army2.IsDefeated)
            return Army1;
        if (Army1.IsDefeated)
            return Army2;
        return null;
    }
}
