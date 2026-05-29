using System.Text.Json.Serialization;
using BattleGame.Units;

namespace BattleGame;

// Армия — коллекция юнитов с логикой проверки состояния.
// Не содержит логику отображения (принцип S — Single Responsibility).
public class Army
{
    public string Name { get; set; } = string.Empty;
    public List<Unit> Units { get; set; } = new();

    // Вычисляемые свойства — не сериализуются, восстанавливаются из Units
    [JsonIgnore] public bool IsDefeated => !Units.Any(u => u.IsAlive);
    [JsonIgnore] public Unit? FirstUnit => Units.Count > 0 ? Units[0] : null;
    [JsonIgnore] public Unit? FirstAliveUnit => Units.FirstOrDefault(u => u.IsAlive);
    [JsonIgnore] public int TotalPrice => Units.Sum(u => u.Price);

    // Удаляет мёртвых юнитов из строя — вызывается в фазе очистки.
    // Оставшиеся юниты сдвигаются вперёд.
    public void RemoveDeadUnits()
    {
        Units.RemoveAll(u => !u.IsAlive);
    }
}
