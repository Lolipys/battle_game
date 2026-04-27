using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Strategies;

// Режим 3 — стенка на стенку.
// Каждый юнит первой армии бьёт юнита из второй на той же позиции.
// Если на позиции у врага никого нет (армия короче) — этот юнит может использовать SA в фазе способностей.
public class WallStrategy : IBattleStrategy
{
    public string Name => "Стенка на стенку";
    public string Description => "Все юниты бьются параллельно: i-й первой армии vs i-й второй.";

    public void ExecuteMelee(Battlefield bf)
    {
        bf.Logger.PhaseHeader($"Ближний бой — {Name}");

        int width = Math.Min(bf.Army1.Units.Count, bf.Army2.Units.Count);

        for (int slot = 0; slot < width; slot++)
        {
            var attacker = bf.Army1.Units[slot];
            var defender = bf.Army2.Units[slot];
            if (!attacker.IsAlive || !defender.IsAlive) continue;

            int hpBefore = defender.Health;
            int dmg = attacker.Attack(defender);
            bf.Logger.MeleeAttack(attacker, defender, dmg, hpBefore);
            bf.NotifyDamage(defender, bf.Army2, hpBefore - defender.Health, hpBefore);

            if (!defender.IsAlive)
            {
                bf.NotifyDeath(defender, bf.Army2);
                continue;
            }

            int hp2 = attacker.Health;
            int dmg2 = defender.Attack(attacker);
            bf.Logger.CounterAttack(defender, attacker, dmg2, hp2);
            bf.NotifyDamage(attacker, bf.Army1, hp2 - attacker.Health, hp2);

            if (!attacker.IsAlive)
                bf.NotifyDeath(attacker, bf.Army1);
        }
    }
}
