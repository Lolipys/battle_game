using System.Text.Json;

namespace BattleGame.Services;

public static class GameSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void Save(Battlefield battlefield, string filePath)
    {
        try
        {
            if (Directory.Exists(filePath))
                filePath = Path.Combine(filePath, "battle_save.json");

            if (!Path.HasExtension(filePath))
                filePath += ".json";

            string json = JsonSerializer.Serialize(battlefield, Options);
            File.WriteAllText(filePath, json);
            Console.WriteLine($"  Игра сохранена: {filePath}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Ошибка сохранения: {ex.Message}");
            Console.ResetColor();
        }
    }

    public static Battlefield? Load(string filePath)
    {
        if (!Path.HasExtension(filePath))
            filePath += ".json";

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Файл не найден: {filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var battlefield = JsonSerializer.Deserialize<Battlefield>(json, Options);

            if (battlefield != null)
                Console.WriteLine($"  Игра загружена: {filePath} (ход {battlefield.TurnNumber})");

            return battlefield;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Ошибка загрузки: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}
