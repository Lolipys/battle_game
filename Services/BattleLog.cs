using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services;

// Реализация IBattleLogger — цветной вывод боевых событий в консоль.
// Принцип S (SOLID): отвечает ТОЛЬКО за отображение, не содержит логику боя.
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

    // Cyan — атака, видно кто кого бьёт
    public void MeleeAttack(Unit attacker, Unit defender, int damage)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {attacker.Name} -> атакует -> {defender.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон: {attacker.Damage} - {defender.Defense} (защита) = {damage}");
        Console.WriteLine($"    {defender.Name}: {defender.Health + damage} -> {defender.Health} HP");
        Console.ResetColor();
    }

    // Magenta — ответный удар, визуально отличается от основной атаки
    public void CounterAttack(Unit defender, Unit attacker, int damage)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  {defender.Name} -> ответный удар -> {attacker.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон: {defender.Damage} - {attacker.Defense} (защита) = {damage}");
        Console.WriteLine($"    {attacker.Name}: {attacker.Health + damage} -> {attacker.Health} HP");
        Console.ResetColor();
    }

    // Green — спецспособность, показывает позицию и дальность стрелка
    public void AbilityUsed(Unit user, Unit target, int damage, int position, int range)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {user.Name} (поз. {position}, дальность {range}) -> стреляет в {target.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Урон стрелой: {damage}");
        Console.WriteLine($"    {target.Name}: {target.Health + damage} -> {target.Health} HP");
        Console.ResetColor();
    }

    // DarkGray — не достаёт, менее важная информация
    public void AbilityOutOfRange(Unit user, int position, int range)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {user.Name} (поз. {position}, дальность {range}) -- не достаёт до врага");
        Console.ResetColor();
    }

    // Red — гибель юнита, сразу бросается в глаза
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

    // Blue — лечение союзника
    public void HealUsed(Unit healer, Unit target, int healAmount, int position, int range)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  {healer.Name} (поз. {position}, дальность {range}) -> лечит {target.Name}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"    Восстановлено: +{healAmount} HP");
        Console.WriteLine($"    {target.Name}: {target.Health - healAmount} -> {target.Health} HP");
        Console.ResetColor();
    }

    // DarkCyan — клонирование
    public void CloneUsed(Unit wizard, Unit original, Unit clone, int position)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  {wizard.Name} (поз. {position}) -> клонировал {original.Name}!");
        Console.WriteLine($"    Создан клон: {clone}");
        Console.ResetColor();
    }

    // DarkGray — клонирование не сработало
    public void CloneFailed(Unit wizard, int position)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {wizard.Name} (поз. {position}) -- клонирование не сработало");
        Console.ResetColor();
    }

    // DarkYellow — щит поглотил урон
    public void ShieldAbsorbed(Unit target, int absorbed, int shieldRemaining)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"    Щит поглотил: {absorbed} урона (осталось щита: {shieldRemaining})");
        Console.ResetColor();
    }

    public void ShieldDestroyed(Unit target)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"    *** Щит {target.Name} разрушен! ***");
        Console.ResetColor();
    }

    // Вывод состава обеих армий — вызывается после каждого хода и по запросу
    public void PrintArmyStatus(Army army1, Army army2)
    {
        Console.WriteLine();
        PrintArmy(army1);
        PrintArmy(army2);
        Console.WriteLine();
    }

    private static void PrintArmy(Army army)
    {
        Console.WriteLine($"  {army.Name} ({army.Units.Count} юнитов, цена: {army.Units.Sum(u => u.Price):F0}):");
        for (int i = 0; i < army.Units.Count; i++)
        {
            Console.WriteLine($"    {i + 1}. {army.Units[i]}");
        }
    }
}
