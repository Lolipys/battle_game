using BattleGame.Units;

namespace BattleGame.Services;

// Абстрактная фабрика: определяет контракт создания юнитов.
// Каждая реализация создаёт юнитов по-своему (случайно, сбалансированно и т.д.).
// Позволяет расширять фабрики без изменения существующего кода (принцип O — SOLID).
public interface IUnitFactory
{
    Unit CreateHeavyInfantry(int number, string tag);
    Unit CreateLightInfantry(int number, string tag);
    Unit CreateArcher(int number, string tag);
    Unit CreateHealer(int number, string tag);
    Unit CreateWizard(int number, string tag);
    Unit CreateGulyayGorod(int number, string tag);

    // Фабричный метод: создаёт случайного юнита любого типа
    Unit CreateRandom(int number, string tag);
}
