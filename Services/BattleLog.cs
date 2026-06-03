using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services;

public class BattleLog : IBattleLogger
{
    public void TurnHeader(int turn)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n{"",2}============== Ход {turn} ==============");
        Console.ResetColor();
    }

    public void PhaseHeader(string phase)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\n  --- {phase} ---");
        Console.ResetColor();
    }

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    public void MeleeAttack(Unit attacker, Unit defender, int damage, int hpBefore)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {attacker.Name} -> атакует -> {defender.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон: {attacker.Damage} - {defender.Defense} (защита) = {damage}");
        Console.WriteLine($"    {defender.Name}: {hpBefore} -> {defender.Health} HP");
        Console.ResetColor();
    }

    public void CounterAttack(Unit defender, Unit attacker, int damage, int hpBefore)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  {defender.Name} -> ответный удар -> {attacker.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон: {defender.Damage} - {attacker.Defense} (защита) = {damage}");
        Console.WriteLine($"    {attacker.Name}: {hpBefore} -> {attacker.Health} HP");
        Console.ResetColor();
    }

    public void AbilityUsed(Unit user, Unit target, int damage, int position, int range, int hpBefore)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {user.Name} (поз. {position}, дальность {range}) -> стреляет в {target.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон стрелой: {damage}");
        Console.WriteLine($"    {target.Name}: {hpBefore} -> {target.Health} HP");
        Console.ResetColor();
    }

    public void AbilityOutOfRange(Unit user, int position, int range)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {user.Name} (поз. {position}, дальность {range}) -- не достаёт до врага");
        Console.ResetColor();
    }

    // вызывается из DeathObserver, не напрямую из Battlefield
    public void UnitDied(Unit unit, string armyName)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  *** {unit.Name} из {armyName} погиб! ***");
        Console.ResetColor();
    }

    public void CleanupResult(int dead1, int dead2, Army army1, Army army2)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Потери: {army1.Name} -{dead1}, {army2.Name} -{dead2}");
        Console.WriteLine($"  Осталось: {army1.Name} = {army1.Units.Count}, {army2.Name} = {army2.Units.Count}");
        Console.ResetColor();
    }

    public void GameOver(Army? winner)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n  ========== ИГРА ОКОНЧЕНА ==========");
        if (winner != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Победитель: {winner.Name}!");
            Console.WriteLine($"  Выжившие ({winner.Units.Count}):");
            foreach (var unit in winner.Units)
                Console.WriteLine($"    {unit}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Ничья! Обе армии уничтожены.");
        }
        Console.ResetColor();
    }

    public void Draw(string reason)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  ========== НИЧЬЯ ==========");
        Console.WriteLine($"  {reason}");
        Console.ResetColor();
    }

    public void HealUsed(Unit healer, Unit target, int healAmount, int position, int range)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  {healer.Name} (поз. {position}, дальность {range}) -> лечит {target.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Восстановлено: +{healAmount} HP");
        Console.WriteLine($"    {target.Name}: {target.Health - healAmount} -> {target.Health} HP");
        Console.ResetColor();
    }

    public void CloneUsed(Unit wizard, Unit original, Unit clone, int position)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  {wizard.Name} (поз. {position}) -> клонировал {original.Name}!");
        Console.WriteLine($"    Создан клон: {clone}");
        Console.ResetColor();
    }

    public void CloneFailed(Unit wizard, int position)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {wizard.Name} (поз. {position}) -- клонирование не сработало");
        Console.ResetColor();
    }

    public void BuffApplied(Unit squire, Unit target, string buffName)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {squire.Name} -> надел [{buffName}] на {target.Name}");
        Console.ResetColor();
    }

    public void BuffStripped(Unit target, string buffName)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"    Сбит бафф [{buffName}] с {target.Name}");
        Console.ResetColor();
    }

    public void StrategyChanged(string strategyName)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n  >> Построение изменено: {strategyName}");
        Console.ResetColor();
    }

    public void PrintArmyStatus(Army army1, Army army2)
    {
        Console.WriteLine();
        PrintArmy(army1);
        PrintArmy(army2);
        Console.WriteLine();
    }

    private static void PrintArmy(Army army)
    {
        Console.WriteLine($"  {army.Name} ({army.Units.Count} юнитов, цена: {army.Units.Sum(u => u.Price)}):");
        for (int i = 0; i < army.Units.Count; i++)
        {
            Console.WriteLine($"    {i + 1}. {army.Units[i]}");
        }
    }
}
