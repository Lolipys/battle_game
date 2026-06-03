using BattleGame.Units;

namespace BattleGame.Services;

// Singleton + Abstract Factory: один экземпляр, делегирует создание юнитов IUnitFactory
public class ArmyFactory
{
    private static ArmyFactory? _instance;
    public static ArmyFactory Instance => _instance ??= new ArmyFactory(new RandomUnitFactory());

    private readonly IUnitFactory _unitFactory;
    private readonly Random _rng = new();

    private ArmyFactory(IUnitFactory unitFactory)
    {
        _unitFactory = unitFactory;
    }

    public Army CreateRandom(string name, int unitCount, string tag = "")
    {
        var army = new Army { Name = name };

        for (int i = 0; i < unitCount; i++)
            army.Units.Add(_unitFactory.CreateRandom(i + 1, tag));

        return army;
    }

    public Army CreateByBudget(string name, int budget, string tag = "")
    {
        var army = new Army { Name = name };
        int number = 1;

        while (budget > 0)
        {
            Unit unit = _unitFactory.CreateRandom(number, tag);

            int attempts = 0;
            while (unit.Price > budget && attempts < 10)
            {
                unit = _unitFactory.CreateRandom(number, tag);
                attempts++;
            }

            if (unit.Price > budget) break;

            army.Units.Add(unit);
            budget -= unit.Price;
            number++;
        }

        return army;
    }

    public Army CreateManualByBudget(string name, int budget, string tag = "")
    {
        var army = new Army { Name = name };
        int number = 1;

        Console.WriteLine($"\nСоздание армии \"{name}\" (бюджет: {budget})");
        Console.WriteLine("Примерные цены юнитов:");
        Console.WriteLine($"  1. Тяжёлый пехотинец  ~{CalcPrice(HeavyInfantry.BaseDamage, HeavyInfantry.BaseDefense, HeavyInfantry.BaseHealth)}");
        Console.WriteLine($"  2. Лёгкий пехотинец   ~{CalcPrice(LightInfantry.BaseDamage, LightInfantry.BaseDefense, LightInfantry.BaseHealth)}");
        Console.WriteLine($"  3. Лучник             ~{CalcPrice(Archer.BaseDamage, Archer.BaseDefense, Archer.BaseHealth)}");
        Console.WriteLine($"  4. Хилер              ~{CalcPrice(Healer.BaseDamage, Healer.BaseDefense, Healer.BaseHealth)}");
        Console.WriteLine($"  5. Маг                ~{CalcPrice(Wizard.BaseDamage, Wizard.BaseDefense, Wizard.BaseHealth)}");
        Console.WriteLine($"  6. Гуляй-город        ~{CalcPrice(0, GulyayGorodAdapter.BaseDefense, GulyayGorodAdapter.BaseHealth)}");

        while (budget > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Осталось бюджета: {budget}");
            Console.ResetColor();
            Console.WriteLine($"  Юнит {number}:");
            Console.WriteLine("  1. Тяжёлый  2. Лёгкий  3. Лучник  4. Хилер  5. Маг  6. Гуляй-город  0. Завершить набор");
            Console.Write("  > ");
            int type = ReadInt(0, 6);

            if (type == 0) break;

            Unit unit = type switch
            {
                1 => _unitFactory.CreateHeavyInfantry(number, tag),
                2 => _unitFactory.CreateLightInfantry(number, tag),
                3 => _unitFactory.CreateArcher(number, tag),
                4 => _unitFactory.CreateHealer(number, tag),
                5 => _unitFactory.CreateWizard(number, tag),
                6 => _unitFactory.CreateGulyayGorod(number, tag),
                _ => _unitFactory.CreateRandom(number, tag)
            };

            Console.WriteLine($"  Создан: {unit} (цена: {unit.Price})");
            Console.Write("  Настроить характеристики? [y/n]: ");
            string? customAnswer = Console.ReadLine()?.Trim().ToLower();
            if (customAnswer == "y")
                unit = CustomizeUnit(unit, number, tag);

            if (unit.Price > budget)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  Не хватает бюджета! {unit.Name} стоит {unit.Price}, осталось {budget}");
                Console.ResetColor();
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Куплен: {unit} (цена: {unit.Price})");
            Console.ResetColor();

            army.Units.Add(unit);
            budget -= unit.Price;
            number++;
        }

        if (army.Units.Count == 0)
        {
            Console.WriteLine("  Армия пуста! Добавлен один случайный юнит.");
            army.Units.Add(_unitFactory.CreateRandom(1, tag));
        }

        Console.WriteLine($"  Итого: {army.Units.Count} юнитов, потрачено: {army.TotalPrice}");
        return army;
    }

    private static int CalcPrice(int damage, int defense, int health)
    {
        return damage * 2 + (int)Math.Ceiling(defense * 1.5) + health;
    }

    private Unit CustomizeUnit(Unit template, int number, string tag)
    {
        string MakeName(string type) =>
            string.IsNullOrEmpty(tag) ? $"{type}#{number}" : $"{tag}:{type}#{number}";

        if (template is GulyayGorodAdapter)
        {
            Console.Write($"    Защита [{template.Defense}]: ");
            int def = ReadIntOrDefault(template.Defense);
            Console.Write($"    HP [{template.Health}]: ");
            int hp = ReadIntOrDefault(template.Health);
            return new GulyayGorodAdapter(MakeName("ГуляйГород"), def, hp);
        }

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
            return new Archer(MakeName("Лучник"), damage, defense, health, range, power);
        }

        if (template is Healer healer)
        {
            Console.Write($"    Дальность [{healer.Range}]: ");
            int range = ReadIntOrDefault(healer.Range);
            Console.Write($"    Сила лечения [{healer.Power}]: ");
            int power = ReadIntOrDefault(healer.Power);
            return new Healer(MakeName("Хилер"), damage, defense, health, range, power);
        }

        if (template is Wizard wizard)
        {
            Console.Write($"    Дальность [{wizard.Range}]: ");
            int range = ReadIntOrDefault(wizard.Range);
            Console.Write($"    Шанс клона % [{wizard.CloneChance}]: ");
            int cloneChance = ReadIntOrDefault(wizard.CloneChance);
            return new Wizard(MakeName("Маг"), damage, defense, health, range, cloneChance);
        }

        if (template is HeavyInfantry)
            return new HeavyInfantry(MakeName("Тяжёлый"), damage, defense, health);

        return new LightInfantry(MakeName("Лёгкий"), damage, defense, health);
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
        if (string.IsNullOrEmpty(input)) return defaultValue;
        return int.TryParse(input, out int value) ? value : defaultValue;
    }
}
