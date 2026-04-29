using BattleGame;
using BattleGame.Interfaces;
using BattleGame.Services;
using BattleGame.Services.Commands;
using BattleGame.Services.Observers;
using BattleGame.Services.Strategies;

// --- Настройка ---
string savesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "saves"));
Directory.CreateDirectory(savesDir);

// Логгер — теперь без SoundLoggerProxy. Звук был выкинут вместе с проксями (демо 3).
IBattleLogger logger = new BattleLog();
Battlefield? battlefield = null;
CommandManager? commands = null;

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
            commands = AttachServices(battlefield);
            RunGameLoop(battlefield, commands);
            break;

        case "2":
            battlefield = LoadGame();
            if (battlefield != null)
            {
                battlefield.Logger = logger;
                battlefield.SetStrategy(StrategyResolver.Resolve(battlefield.StrategyName));
                commands = AttachServices(battlefield);
                RunGameLoop(battlefield, commands);
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

// --- Создание услуг (Observer + Command) ---
CommandManager AttachServices(Battlefield bf)
{
    bf.Subscribe(new DeathObserver(logger));   // 1-й наблюдатель — смерть юнитов
    bf.Subscribe(new DamageObserver());        // 2-й наблюдатель — урон + правило ничьи
    return new CommandManager();
}

Army CreateArmy(string name, string tag, int budget)
{
    var factory = ArmyFactory.Instance;

    Console.WriteLine($"\n--- {name} (бюджет: {budget}) ---");
    Console.WriteLine("1. Случайная армия");
    Console.WriteLine("2. Создать вручную");
    Console.Write("> ");

    string? mode = Console.ReadLine()?.Trim();
    return mode == "2"
        ? factory.CreateManualByBudget(name, budget, tag)
        : factory.CreateByBudget(name, budget, tag);
}

Battlefield CreateGame()
{
    Console.Write("\nБюджет на каждую армию (например, 500): ");
    int budget = ReadPositiveInt();

    var bf = new Battlefield(logger)
    {
        Army1 = CreateArmy("Армия 1", "A1", budget),
        Army2 = CreateArmy("Армия 2", "A2", budget)
    };

    // По умолчанию — узкий мост; можно сменить в меню
    bf.SetStrategy(new BridgeStrategy());

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

Battlefield? LoadGame()
{
    var files = Directory.GetFiles(savesDir, "*.json");
    if (files.Length == 0)
    {
        Console.WriteLine("  Нет сохранений.");
        return null;
    }

    Console.WriteLine("\nДоступные сохранения:");
    for (int i = 0; i < files.Length; i++)
        Console.WriteLine($"  {i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
    Console.WriteLine("  0. Назад в главное меню");
    Console.Write("\nНомер или имя файла: ");
    string? input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input) || input == "0") return null;

    string filePath;
    if (int.TryParse(input, out int index) && index >= 1 && index <= files.Length)
        filePath = files[index - 1];
    else
    {
        filePath = Path.Combine(savesDir, input);
        if (!Path.HasExtension(filePath)) filePath += ".json";
    }

    return GameSerializer.Load(filePath);
}

// Игровой цикл: команды (Execute/Undo/Redo/Reset) + смена стратегии в любой момент
void RunGameLoop(Battlefield bf, CommandManager cmds)
{
    while (true)
    {
        if (bf.IsGameOver)
        {
            Console.WriteLine("Игра окончена. Возврат в главное меню.");
            break;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n--- Игровое меню (построение: {bf.Strategy.Name}) ---");
        Console.ResetColor();
        Console.WriteLine("1. Сделать ход");
        Console.WriteLine("2. Играть до конца");
        Console.WriteLine("3. Показать армии");
        Console.WriteLine("4. Сохранить игру");
        Console.WriteLine("5. Перепостроение (сменить стратегию)");
        Console.WriteLine($"6. Отменить ход  [история: {cmds.HistoryDepth}]");
        Console.WriteLine("7. Повторить ход");
        Console.WriteLine("8. Сбросить к началу");
        Console.WriteLine("0. Вернуться в главное меню");
        Console.Write("\n> ");

        string? action = Console.ReadLine()?.Trim();
        if (action == null) return;

        switch (action)
        {
            case "1":
                cmds.Execute(new MakeTurnCommand(bf, bf.DamageObserver));
                break;

            case "2":
                cmds.Execute(new PlayToEndCommand(bf, bf.DamageObserver));
                break;

            case "3":
                bf.PrintArmyStatus();
                break;

            case "4":
                SaveGame(bf);
                break;

            case "5":
                ChangeStrategyMenu(bf, cmds);
                break;

            case "6":
                if (!cmds.Undo()) Console.WriteLine("Нечего отменять.");
                else Console.WriteLine($"Отменено. Ход: {bf.TurnNumber}");
                break;

            case "7":
                if (!cmds.Redo()) Console.WriteLine("Нечего повторять.");
                else Console.WriteLine($"Повторено. Ход: {bf.TurnNumber}");
                break;

            case "8":
                cmds.Reset();
                Console.WriteLine($"Сброшено к началу. Ход: {bf.TurnNumber}");
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Неверный ввод.");
                break;
        }
    }
}

void ChangeStrategyMenu(Battlefield bf, CommandManager cmds)
{
    var strategies = StrategyResolver.All();
    Console.WriteLine("\nВыбор построения:");
    for (int i = 0; i < strategies.Length; i++)
        Console.WriteLine($"  {i + 1}. {strategies[i].Name} — {strategies[i].Description}");
    Console.Write("> ");
    string? input = Console.ReadLine()?.Trim();
    if (!int.TryParse(input, out int idx) || idx < 1 || idx > strategies.Length)
    {
        Console.WriteLine("Неверный выбор.");
        return;
    }
    cmds.Execute(new ChangeStrategyCommand(bf, strategies[idx - 1]));
}

void SaveGame(Battlefield bf)
{
    Console.Write("Имя сохранения (Enter = battle_save): ");
    string? name = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(name)) name = "battle_save";
    string filePath = Path.Combine(savesDir, name);
    GameSerializer.Save(bf, filePath);
}

static int ReadPositiveInt()
{
    while (true)
    {
        if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
            return value;
        Console.Write("Введите положительное число: ");
    }
}
