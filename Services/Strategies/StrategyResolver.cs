using BattleGame.Interfaces;

namespace BattleGame.Services.Strategies;

// Резолвер стратегий по имени — нужен для восстановления при загрузке/Undo.
// Имя сериализуется в Battlefield.StrategyName, объект-стратегия пересоздаётся.
public static class StrategyResolver
{
    public static IBattleStrategy[] All() => new IBattleStrategy[]
    {
        new BridgeStrategy(),
        new WideBridgeStrategy(),
        new WallStrategy()
    };

    public static IBattleStrategy Resolve(string name)
    {
        foreach (var s in All())
            if (s.Name == name) return s;
        return new BridgeStrategy();
    }
}
