using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Strategies;

// Режим 1 - узкий мост
// Бьются только первые юниты обеих армий: A1[0] против A2[0]
// Если защитник погибает - ответного удара нет
public class BridgeStrategy : IBattleStrategy
{
    public string Name => "Узкий мост (1 vs 1)";
    public string Description => "Только первые юниты обеих армий бьются друг с другом.";

    public void ExecuteMelee(Battlefield bf)
    {
        bf.Logger.PhaseHeader($"Ближний бой — {Name}");

        var attacker = bf.Army1.FirstUnit;
        var defender = bf.Army2.FirstUnit;
        if (attacker == null || defender == null)
            return;

        DoExchange(bf, attacker, bf.Army1, defender, bf.Army2);
    }

    // Удар + ответный удар, если защитник выжил
    private static void DoExchange(Battlefield bf, Unit attacker, Army aArmy, Unit defender, Army dArmy)
    {
        if (!attacker.IsAlive || !defender.IsAlive) return;

        int hpBefore = defender.Health;
        int dmg = attacker.Attack(defender);
        bf.Logger.MeleeAttack(attacker, defender, dmg, hpBefore);
        bf.NotifyDamage(defender, dArmy, hpBefore - defender.Health, hpBefore);

        if (!defender.IsAlive)
        {
            bf.NotifyDeath(defender, dArmy);
            return;
        }

        int hp2 = attacker.Health;
        int dmg2 = defender.Attack(attacker);
        bf.Logger.CounterAttack(defender, attacker, dmg2, hp2);
        bf.NotifyDamage(attacker, aArmy, hp2 - attacker.Health, hp2);

        if (!attacker.IsAlive)
            bf.NotifyDeath(attacker, aArmy);
    }
}
