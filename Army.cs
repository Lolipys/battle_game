using BattleGame.Units;

namespace BattleGame;

public class Army
{
    public string Name { get; set; } = string.Empty;
    public List<Unit> Units { get; set; } = new();

    public bool IsDefeated => !Units.Any(u => u.IsAlive);

    public Unit? FirstUnit => Units.Count > 0 ? Units[0] : null;

    public Unit? FirstAliveUnit => Units.FirstOrDefault(u => u.IsAlive);

    public decimal TotalPrice => Units.Sum(u => u.Price);

    public void RemoveDeadUnits()
    {
        Units.RemoveAll(u => !u.IsAlive);
    }
}
