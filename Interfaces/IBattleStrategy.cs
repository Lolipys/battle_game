namespace BattleGame.Interfaces;

// Strategy: алгоритм фазы ближнего боя. Bridge/WideBridge/Wall — три реализации
// Стратегию можно сменить в любой момент через ChangeStrategyCommand
public interface IBattleStrategy
{
    string Name { get; }
    string Description { get; }
    void ExecuteMelee(Battlefield battlefield);
}
