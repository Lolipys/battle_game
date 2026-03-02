using BattleGame;
using BattleGame.Interfaces;
using BattleGame.Services;

string savesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "saves"));
Directory.CreateDirectory(savesDir);
string SaveFile = Path.Combine(savesDir, "battle_save.json");

IBattleLogger logger = new BattleLog();
Battlefield? battlefield = null;

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("\n=== АРЕНА ===");
    Console.ResetColor();
    Console.WriteLine("1. Новая игра (случайные армии)");
    Console.WriteLine("2. Новая игра (создать вручную)");
    Console.WriteLine("3. Загрузить игру");
    Console.WriteLine("0. Выход");
    Console.Write("\n> ");

    string? choice = Console.ReadLine()?.Trim();
    if (choice == null) break;

    switch (choice)
    {
        case "1":
            battlefield = CreateRandomGame();
            RunGameLoop(battlefield);
            break;

        case "2":
            battlefield = CreateManualGame();
            RunGameLoop(battlefield);
            break;

            case "3":
            battlefield = GameSerializer.Load(SaveFile);
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

Battlefield CreateRandomGame()
{
    Console.Write("Размер армий (количество юнитов): ");
    int size = ReadPositiveInt();

    var bf = new Battlefield(logger)
    {
        Army1 = ArmyFactory.CreateRandom("Армия 1", size, "A1"),
        Army2 = ArmyFactory.CreateRandom("Армия 2", size, "A2")
    };

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

Battlefield CreateManualGame()
{
    var bf = new Battlefield(logger)
    {
        Army1 = ArmyFactory.CreateManual("Армия 1", "A1"),
        Army2 = ArmyFactory.CreateManual("Армия 2", "A2")
    };

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

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
                GameSerializer.Save(battlefield, SaveFile);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Неверный ввод.");
                break;
        }
    }
}

int ReadPositiveInt()
{
    while (true)
    {
        if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
            return value;
        Console.Write("Введите положительное число: ");
    }
}
