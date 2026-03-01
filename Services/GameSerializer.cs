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
        string json = JsonSerializer.Serialize(battlefield, Options);
        File.WriteAllText(filePath, json);
        Console.WriteLine($"  Игра сохранена: {filePath}");
    }

    public static Battlefield? Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  Файл не найден: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        var battlefield = JsonSerializer.Deserialize<Battlefield>(json, Options);

        if (battlefield != null)
            Console.WriteLine($"  Игра загружена: {filePath} (ход {battlefield.TurnNumber})");

        return battlefield;
    }
}
