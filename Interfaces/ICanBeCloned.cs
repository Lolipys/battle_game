using BattleGame.Units;

namespace BattleGame.Interfaces;

// Юнит может быть клонирован магом.
// Реализуют: LightInfantry, Archer.
// НЕ реализуют: HeavyInfantry, Healer, GulyayGorod, Wizard.
public interface ICanBeCloned
{
    // Создаёт копию юнита с текущими характеристиками
    Unit Clone();
}
