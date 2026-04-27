using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Strategies;

// Режим 2 — широкий мост. Колонна по трое.
// Первые три юнита бьются против первых трёх врагов (1A↔1B, 2A↔2B, 3A↔3B).
// Если первая линия пуста (юнит нашёл противника напротив) — Special Ability в фазе SA.
public class WideBridgeStrategy : IBattleStrategy
{
    public const int Width = 3;

    public string Name => "Широкий мост (по трое)";
    public string Description => $"Бой колоннами шириной {Width}: 1A↔1B, 2A↔2B, 3A↔3B.";

    public void ExecuteMelee(Battlefield bf)
    {
        bf.Logger.PhaseHeader($"Ближний бой — {Name}");

        for (int slot = 0; slot < Width; slot++)
        {
            if (slot >= bf.Army1.Units.Count || slot >= bf.Army2.Units.Count)
                break;

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
                continue;  // ответного удара нет
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
