using BattleGame.Units;

namespace BattleGame;

// Армия — коллекция юнитов с логикой проверки состояния.
// Не содержит логику отображения (принцип S — Single Responsibility).
public class Army
{
    public string Name { get; set; } = string.Empty;
    public List<Unit> Units { get; set; } = new();

    // Армия разгромлена, если ни одного живого юнита не осталось
    public bool IsDefeated => !Units.Any(u => u.IsAlive);

    // Первый юнит в строю — участвует в ближнем бою
    public Unit? FirstUnit => Units.Count > 0 ? Units[0] : null;

    // Первый ЖИВОЙ юнит — цель для спецспособностей (стрелки бьют именно в него)
    public Unit? FirstAliveUnit => Units.FirstOrDefault(u => u.IsAlive);

    // Суммарная стоимость армии для сравнения баланса
    public int TotalPrice => Units.Sum(u => u.Price);

    // Удаляет мёртвых юнитов из строя — вызывается в фазе очистки.
    // Оставшиеся юниты сдвигаются вперёд.
    public void RemoveDeadUnits()
    {
        Units.RemoveAll(u => !u.IsAlive);
    }
}
