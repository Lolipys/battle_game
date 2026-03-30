using BattleGame;
using BattleGame.Interfaces;
using BattleGame.Services;

// --- Настройка ---
// Папка saves/ рядом с проектом, создаётся автоматически
string savesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "saves"));
Directory.CreateDirectory(savesDir);

// Dependency Injection: создаём логгер и оборачиваем в звуковой прокси
// BattleLog → SoundLoggerProxy → Battlefield (два паттерна: DI + Proxy)
IBattleLogger logger = new SoundLoggerProxy(new BattleLog());
Battlefield? battlefield = null;

// --- Главное меню ---
while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("\n=== АРЕНА ===");
    Console.ResetColor();
    Console.WriteLine("1. Новая игра");
    Console.WriteLine("2. Загрузить игру");
    Console.WriteLine("0. Выход");
    Console.Write("\n> ");

    string? choice = Console.ReadLine()?.Trim();
    if (choice == null) break;

    switch (choice)
    {
        case "1":
            battlefield = CreateGame();
            RunGameLoop(battlefield);
            break;

        case "2":
            battlefield = LoadGame();
            if (battlefield != null)
            {
                battlefield.Logger = logger;
                RunGameLoop(battlefield);
            }
            break;

        case "0":
            Console.WriteLine("Выход.");
            return;

        default:
            Console.WriteLine("Неверный ввод.");
            break;
    }
}

// --- Локальные функции ---

// Создание армии: каждый игрок выбирает способ (случайно или вручную)
Army CreateArmy(string name, string tag, int budget)
{
    var factory = ArmyFactory.Instance;

    Console.WriteLine($"\n--- {name} (бюджет: {budget}) ---");
    Console.WriteLine("1. Случайная армия");
    Console.WriteLine("2. Создать вручную");
    Console.Write("> ");

    string? mode = Console.ReadLine()?.Trim();

    if (mode == "2")
        return factory.CreateManualByBudget(name, budget, tag);

    // По умолчанию — случайная в рамках бюджета
    return factory.CreateByBudget(name, budget, tag);
}

// Новая игра: задаётся общий бюджет, затем каждый игрок собирает армию
Battlefield CreateGame()
{
    Console.Write("\nБюджет на каждую армию (например, 500): ");
    int budget = ReadPositiveInt();

    var bf = new Battlefield(logger)
    {
        Army1 = CreateArmy("Армия 1", "A1", budget),
        Army2 = CreateArmy("Армия 2", "A2", budget)
    };

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

// Загрузка: показываем список сохранений или ввод имени
Battlefield? LoadGame()
{
    // Показываем доступные сохранения
    var files = Directory.GetFiles(savesDir, "*.json");
    if (files.Length == 0)
    {
        Console.WriteLine("  Нет сохранений.");
        return null;
    }

    Console.WriteLine("\nДоступные сохранения:");
    for (int i = 0; i < files.Length; i++)
    {
        Console.WriteLine($"  {i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
    }
    Console.WriteLine("  0. Назад в главное меню");
    Console.Write("\nНомер или имя файла: ");
    string? input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input) || input == "0") return null;

    // Если ввели число — выбор по номеру
    string filePath;
    if (int.TryParse(input, out int index) && index >= 1 && index <= files.Length)
    {
        filePath = files[index - 1];
    }
    else
    {
        filePath = Path.Combine(savesDir, input);
        if (!Path.HasExtension(filePath)) filePath += ".json";
    }

    return GameSerializer.Load(filePath);
}

// Игровой цикл: ход, авто-бой, просмотр армий, сохранение
void RunGameLoop(Battlefield battlefield)
{
    while (true)
    {
        if (battlefield.IsGameOver)
        {
            Console.WriteLine("Игра окончена. Возврат в главное меню.");
            break;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n--- Игровое меню ---");
        Console.ResetColor();
        Console.WriteLine("1. Сделать ход");
        Console.WriteLine("2. Играть до конца");
        Console.WriteLine("3. Показать армии");
        Console.WriteLine("4. Сохранить игру");
        Console.WriteLine("0. Вернуться в главное меню");
        Console.Write("\n> ");

        string? action = Console.ReadLine()?.Trim();
        if (action == null) return;

        switch (action)
        {
            case "1":
                battlefield.MakeTurn();
                break;

            case "2":
                battlefield.PlayToEnd();
                break;

            case "3":
                battlefield.PrintArmyStatus();
                break;

            case "4":
                SaveGame(battlefield);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Неверный ввод.");
                break;
        }
    }
}

// Сохранение с выбором имени
void SaveGame(Battlefield battlefield)
{
    Console.Write("Имя сохранения (Enter = battle_save): ");
    string? name = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(name)) name = "battle_save";

    string filePath = Path.Combine(savesDir, name);
    GameSerializer.Save(battlefield, filePath);
}

// static — не захватывает внешние переменные, в отличие от остальных функций
static int ReadPositiveInt()
{
    while (true)
    {
        if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
            return value;
        Console.Write("Введите положительное число: ");
    }
}
