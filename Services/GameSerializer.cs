using System.Text.Json;

namespace BattleGame.Services;

// Сериализация/десериализация игры в JSON.
// static — чистая утилита без состояния.
public static class GameSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true  // читаемый JSON с отступами
    };

    // Сохранение состояния игры в файл
    public static void Save(Battlefield battlefield, string filePath)
    {
        try
        {
            // Защита: если указали папку вместо файла — добавляем имя файла
            if (Directory.Exists(filePath))
                filePath = Path.Combine(filePath, "battle_save.json");

            // Защита: если забыли расширение — добавляем .json
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

    // Загрузка состояния игры из файла
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
            // JsonDerivedType атрибуты в Unit.cs обеспечивают правильную десериализацию наследников
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
