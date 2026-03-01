using BattleGame.Units;

namespace BattleGame.Services;

public static class ArmyFactory
{
    private static readonly Random Rng = new();

    public static Army CreateRandom(string name, int unitCount)
    {
        var army = new Army { Name = name };

        for (int i = 0; i < unitCount; i++)
        {
            army.Units.Add(CreateRandomUnit(i + 1));
        }

        return army;
    }

    public static Army CreateManual(string name)
    {
        var army = new Army { Name = name };

        Console.WriteLine($"\nСоздание армии \"{name}\"");
        Console.Write("Количество юнитов: ");
        int count = ReadInt(1, 20);

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\n  Юнит {i + 1}:");
            Console.WriteLine("  1. Тяжёлый пехотинец");
            Console.WriteLine("  2. Лёгкий пехотинец");
            Console.WriteLine("  3. Лучник");
            Console.Write("  Тип: ");
            int type = ReadInt(1, 3);

            Unit unit = type switch
            {
                1 => CreateHeavyInfantry(i + 1),
                2 => CreateLightInfantry(i + 1),
                3 => CreateArcher(i + 1),
                _ => CreateRandomUnit(i + 1)
            };

            Console.Write($"  Использовать стандартные характеристики? ({unit}) [y/n]: ");
            string? answer = Console.ReadLine()?.Trim().ToLower();

            if (answer == "n")
                unit = CustomizeUnit(unit, i + 1);

            army.Units.Add(unit);
        }

        return army;
    }

    private static Unit CreateRandomUnit(int number)
    {
        return Rng.Next(3) switch
        {
            0 => CreateHeavyInfantry(number),
            1 => CreateLightInfantry(number),
            _ => CreateArcher(number)
        };
    }

    private static HeavyInfantry CreateHeavyInfantry(int number)
    {
        return new HeavyInfantry(
            name: $"Heavy#{number}",
            damage: Vary(HeavyInfantry.BaseDamage, 4),
            defense: Vary(HeavyInfantry.BaseDefense, 3),
            health: Vary(HeavyInfantry.BaseHealth, 20));
    }

    private static LightInfantry CreateLightInfantry(int number)
    {
        return new LightInfantry(
            name: $"Light#{number}",
            damage: Vary(LightInfantry.BaseDamage, 5),
            defense: Vary(LightInfantry.BaseDefense, 2),
            health: Vary(LightInfantry.BaseHealth, 15));
    }

    private static Archer CreateArcher(int number)
    {
        return new Archer(
            name: $"Archer#{number}",
            damage: Vary(Archer.BaseDamage, 2),
            defense: Vary(Archer.BaseDefense, 1),
            health: Vary(Archer.BaseHealth, 10),
            range: Vary(Archer.BaseRange, 1),
            power: Vary(Archer.BasePower, 5));
    }

    private static Unit CustomizeUnit(Unit template, int number)
    {
        Console.Write($"    Урон [{template.Damage}]: ");
        int damage = ReadIntOrDefault(template.Damage);
        Console.Write($"    Защита [{template.Defense}]: ");
        int defense = ReadIntOrDefault(template.Defense);
        Console.Write($"    HP [{template.Health}]: ");
        int health = ReadIntOrDefault(template.Health);

        if (template is Archer archer)
        {
            Console.Write($"    Дальность [{archer.Range}]: ");
            int range = ReadIntOrDefault(archer.Range);
            Console.Write($"    Сила стрелы [{archer.Power}]: ");
            int power = ReadIntOrDefault(archer.Power);
            return new Archer($"Archer#{number}", damage, defense, health, range, power);
        }

        if (template is HeavyInfantry)
            return new HeavyInfantry($"Heavy#{number}", damage, defense, health);

        return new LightInfantry($"Light#{number}", damage, defense, health);
    }

    private static int Vary(int baseValue, int variance)
    {
        return baseValue + Rng.Next(-variance, variance + 1);
    }

    private static int ReadInt(int min, int max)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                return value;
            Console.Write($"  Введите число от {min} до {max}: ");
        }
    }

    private static int ReadIntOrDefault(int defaultValue)
    {
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            return defaultValue;
        return int.TryParse(input, out int value) ? value : defaultValue;
    }
}
