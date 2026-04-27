namespace BattleGame.Interfaces;

// Паттерн Strategy: алгоритм проведения одной фазы боя.
// Реализаций три:
//   BridgeStrategy     — узкий мост, 1 vs 1 (классическое поведение).
//   WideBridgeStrategy — широкий мост, бой колоннами по 3.
//   WallStrategy       — стенка на стенку, все слоты против всех.
// Battlefield делегирует фазу боя выбранной стратегии. Сменить стратегию можно в любой момент через меню.
public interface IBattleStrategy
{
    // Краткое имя для отображения в логе и меню
    string Name { get; }

    // Описание для меню перепостроения
    string Description { get; }

    // Выполнить фазу ближнего боя текущего хода для обеих армий.
    void ExecuteMelee(Battlefield battlefield);
}
