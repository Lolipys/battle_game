using BattleGame.Units;

namespace BattleGame.Services;

// Фабрика армий — создаёт армии случайно или вручную.
// Паттерн Factory: инкапсулирует логику создания объектов.
// static — у фабрики нет состояния, не нужно создавать экземпляр.
public static class ArmyFactory
{
    private static readonly Random Rng = new();

    // Создание армии со случайным составом
    public static Army CreateRandom(string name, int unitCount, string tag = "")
    {
        var army = new Army { Name = name };

        for (int i = 0; i < unitCount; i++)
        {
            Unit unit = CreateRandomUnit(i + 1, tag);

            // 20% шанс получить щит (паттерн Proxy)
            if (Rng.Next(5) == 0)
                unit = new UnitProxy(unit, Vary(UnitProxy.BaseShield, 10));

            army.Units.Add(unit);
        }

        return army;
    }

    // Создание армии вручную: пользователь выбирает тип каждого юнита
    public static Army CreateManual(string name, string tag = "")
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
            Console.WriteLine("  4. Хилер");
            Console.WriteLine("  5. Маг");
            Console.WriteLine("  6. Гуляй-город");
            Console.Write("  Тип: ");
            int type = ReadInt(1, 6);

            // Switch expression — компактный выбор типа юнита
            Unit unit = type switch
            {
                1 => CreateHeavyInfantry(i + 1, tag),
                2 => CreateLightInfantry(i + 1, tag),
                3 => CreateArcher(i + 1, tag),
                4 => CreateHealer(i + 1, tag),
                5 => CreateWizard(i + 1, tag),
                6 => CreateGulyayGorod(i + 1, tag),
                _ => CreateRandomUnit(i + 1, tag)
            };

            Console.Write($"  Использовать стандартные характеристики? ({unit}) [y/n]: ");
            string? answer = Console.ReadLine()?.Trim().ToLower();

            if (answer == "n")
                unit = CustomizeUnit(unit, i + 1, tag);

            // Паттерн Proxy: предложить обернуть юнита в щит
            Console.Write("  Добавить щит? [y/n]: ");
            string? shieldAnswer = Console.ReadLine()?.Trim().ToLower();
            if (shieldAnswer == "y")
                unit = new UnitProxy(unit, Vary(UnitProxy.BaseShield, 10));

            army.Units.Add(unit);
        }

        return army;
    }

    // Случайный выбор типа юнита (6 типов)
    private static Unit CreateRandomUnit(int number, string tag)
    {
        return Rng.Next(6) switch
        {
            0 => CreateHeavyInfantry(number, tag),
            1 => CreateLightInfantry(number, tag),
            2 => CreateArcher(number, tag),
            3 => CreateHealer(number, tag),
            4 => CreateWizard(number, tag),
            _ => CreateGulyayGorod(number, tag)
        };
    }

    // Формат имени: "A1:Heavy#1" — тег армии + тип + порядковый номер
    private static string MakeName(string type, int number, string tag)
    {
        return string.IsNullOrEmpty(tag) ? $"{type}#{number}" : $"{tag}:{type}#{number}";
    }

    private static HeavyInfantry CreateHeavyInfantry(int number, string tag)
    {
        return new HeavyInfantry(
            name: MakeName("Тяжёлый", number, tag),
            damage: Vary(HeavyInfantry.BaseDamage, 4),
            defense: Vary(HeavyInfantry.BaseDefense, 3),
            health: Vary(HeavyInfantry.BaseHealth, 20));
    }

    private static LightInfantry CreateLightInfantry(int number, string tag)
    {
        return new LightInfantry(
            name: MakeName("Лёгкий", number, tag),
            damage: Vary(LightInfantry.BaseDamage, 5),
            defense: Vary(LightInfantry.BaseDefense, 2),
            health: Vary(LightInfantry.BaseHealth, 15));
    }

    private static Archer CreateArcher(int number, string tag)
    {
        return new Archer(
            name: MakeName("Лучник", number, tag),
            damage: Vary(Archer.BaseDamage, 2),
            defense: Vary(Archer.BaseDefense, 1),
            health: Vary(Archer.BaseHealth, 10),
            range: Vary(Archer.BaseRange, 1),
            power: Vary(Archer.BasePower, 5));
    }

    private static Healer CreateHealer(int number, string tag)
    {
        return new Healer(
            name: MakeName("Хилер", number, tag),
            damage: Vary(Healer.BaseDamage, 2),
            defense: Vary(Healer.BaseDefense, 1),
            health: Vary(Healer.BaseHealth, 10),
            range: Vary(Healer.BaseRange, 1),
            power: Vary(Healer.BasePower, 5));
    }

    private static Wizard CreateWizard(int number, string tag)
    {
        return new Wizard(
            name: MakeName("Маг", number, tag),
            damage: Vary(Wizard.BaseDamage, 2),
            defense: Vary(Wizard.BaseDefense, 1),
            health: Vary(Wizard.BaseHealth, 10),
            range: Vary(Wizard.BaseRange, 1),
            cloneChance: Vary(Wizard.BaseCloneChance, 10));
    }

    private static GulyayGorodAdapter CreateGulyayGorod(int number, string tag)
    {
        return new GulyayGorodAdapter(
            name: MakeName("ГуляйГород", number, tag),
            defense: Vary(GulyayGorodAdapter.BaseDefense, 5),
            health: Vary(GulyayGorodAdapter.BaseHealth, 30));
    }

    // Ручная настройка характеристик юнита (Enter = оставить по умолчанию)
    private static Unit CustomizeUnit(Unit template, int number, string tag)
    {
        Console.Write($"    Урон [{template.Damage}]: ");
        int damage = ReadIntOrDefault(template.Damage);
        Console.Write($"    Защита [{template.Defense}]: ");
        int defense = ReadIntOrDefault(template.Defense);
        Console.Write($"    HP [{template.Health}]: ");
        int health = ReadIntOrDefault(template.Health);

        // У лучника — дополнительные параметры дальности и силы стрелы
        if (template is Archer archer)
        {
            Console.Write($"    Дальность [{archer.Range}]: ");
            int range = ReadIntOrDefault(archer.Range);
            Console.Write($"    Сила стрелы [{archer.Power}]: ");
            int power = ReadIntOrDefault(archer.Power);
            return new Archer(MakeName("Лучник", number, tag), damage, defense, health, range, power);
        }

        // У хилера — дальность и сила лечения
        if (template is Healer healer)
        {
            Console.Write($"    Дальность [{healer.Range}]: ");
            int range = ReadIntOrDefault(healer.Range);
            Console.Write($"    Сила лечения [{healer.Power}]: ");
            int power = ReadIntOrDefault(healer.Power);
            return new Healer(MakeName("Хилер", number, tag), damage, defense, health, range, power);
        }

        // У мага — дальность и шанс клонирования
        if (template is Wizard wizard)
        {
            Console.Write($"    Дальность [{wizard.Range}]: ");
            int range = ReadIntOrDefault(wizard.Range);
            Console.Write($"    Шанс клона % [{wizard.CloneChance}]: ");
            int cloneChance = ReadIntOrDefault(wizard.CloneChance);
            return new Wizard(MakeName("Маг", number, tag), damage, defense, health, range, cloneChance);
        }

        // Гуляй-город — только защита и HP (без атаки)
        if (template is GulyayGorodAdapter)
            return new GulyayGorodAdapter(MakeName("ГуляйГород", number, tag), defense, health);

        if (template is HeavyInfantry)
            return new HeavyInfantry(MakeName("Тяжёлый", number, tag), damage, defense, health);

        return new LightInfantry(MakeName("Лёгкий", number, tag), damage, defense, health);
    }

    // Рандомизация: базовое значение ± variance. Пример: Vary(150, 20) → от 130 до 170
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

    // Чтение числа с возможностью оставить значение по умолчанию (пустой Enter)
    private static int ReadIntOrDefault(int defaultValue)
    {
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            return defaultValue;
        return int.TryParse(input, out int value) ? value : defaultValue;
    }
}
