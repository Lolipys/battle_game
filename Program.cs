using BattleGame;
using BattleGame.Services;

const string SaveFile = "battle_save.json";

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
            Console.Write($"Путь к файлу [{SaveFile}]: ");
            string? path = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(path)) path = SaveFile;
            battlefield = GameSerializer.Load(path);
            if (battlefield != null)
                RunGameLoop(battlefield);
            break;

        case "0":
            Console.WriteLine("Выход.");
            return;

        default:
            Console.WriteLine("Неверный ввод.");
            break;
    }
}

static Battlefield CreateRandomGame()
{
    Console.Write("Размер армий (количество юнитов): ");
    int size = ReadPositiveInt();

    var bf = new Battlefield
    {
        Army1 = ArmyFactory.CreateRandom("Армия 1", size),
        Army2 = ArmyFactory.CreateRandom("Армия 2", size)
    };

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

static Battlefield CreateManualGame()
{
    var bf = new Battlefield
    {
        Army1 = ArmyFactory.CreateManual("Армия 1"),
        Army2 = ArmyFactory.CreateManual("Армия 2")
    };

    Console.WriteLine("\nАрмии созданы:");
    bf.PrintArmyStatus();
    return bf;
}

static void RunGameLoop(Battlefield battlefield)
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
                Console.Write($"Путь для сохранения [{SaveFile}]: ");
                string? savePath = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(savePath)) savePath = SaveFile;
                GameSerializer.Save(battlefield, savePath);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Неверный ввод.");
                break;
        }
    }
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
