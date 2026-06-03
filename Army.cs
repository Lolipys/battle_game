using System.Text.Json.Serialization;
using BattleGame.Units;

namespace BattleGame;

public class Army
{
    public string Name { get; set; } = string.Empty;
    public List<Unit> Units { get; set; } = new();

    // вычисляются из Units, сериализовывать не нужно
    [JsonIgnore] public bool IsDefeated => !Units.Any(u => u.IsAlive);
    // позиция 0 — передовая (ближний бой)
    [JsonIgnore] public Unit? FirstUnit => Units.Count > 0 ? Units[0] : null;
    // первый живой — цель для лучников и дальних атак
    [JsonIgnore] public Unit? FirstAliveUnit => Units.FirstOrDefault(u => u.IsAlive);
    [JsonIgnore] public int TotalPrice => Units.Sum(u => u.Price);

    public void RemoveDeadUnits()
    {
        Units.RemoveAll(u => !u.IsAlive);
    }
}
