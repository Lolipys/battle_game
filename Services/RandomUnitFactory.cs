using BattleGame.Units;

namespace BattleGame.Services;

// Конкретная реализация абстрактной фабрики IUnitFactory.
// Создаёт юнитов с рандомизацией характеристик вокруг базовых значений.
public class RandomUnitFactory : IUnitFactory
{
    private readonly Random _rng = new();

    public Unit CreateHeavyInfantry(int number, string tag)
    {
        return new HeavyInfantry(
            name: MakeName("Тяжёлый", number, tag),
            damage: Vary(HeavyInfantry.BaseDamage, 4),
            defense: Vary(HeavyInfantry.BaseDefense, 3),
            health: Vary(HeavyInfantry.BaseHealth, 20));
    }

    public Unit CreateLightInfantry(int number, string tag)
    {
        return new LightInfantry(
            name: MakeName("Лёгкий", number, tag),
            damage: Vary(LightInfantry.BaseDamage, 5),
            defense: Vary(LightInfantry.BaseDefense, 2),
            health: Vary(LightInfantry.BaseHealth, 15));
    }

    public Unit CreateArcher(int number, string tag)
    {
        return new Archer(
            name: MakeName("Лучник", number, tag),
            damage: Vary(Archer.BaseDamage, 2),
            defense: Vary(Archer.BaseDefense, 1),
            health: Vary(Archer.BaseHealth, 10),
            range: Vary(Archer.BaseRange, 1),
            power: Vary(Archer.BasePower, 5));
    }

    public Unit CreateHealer(int number, string tag)
    {
        return new Healer(
            name: MakeName("Хилер", number, tag),
            damage: Vary(Healer.BaseDamage, 2),
            defense: Vary(Healer.BaseDefense, 1),
            health: Vary(Healer.BaseHealth, 10),
            range: Vary(Healer.BaseRange, 1),
            power: Vary(Healer.BasePower, 5));
    }

    public Unit CreateWizard(int number, string tag)
    {
        return new Wizard(
            name: MakeName("Маг", number, tag),
            damage: Vary(Wizard.BaseDamage, 2),
            defense: Vary(Wizard.BaseDefense, 1),
            health: Vary(Wizard.BaseHealth, 10),
            range: Vary(Wizard.BaseRange, 1),
            cloneChance: Vary(Wizard.BaseCloneChance, 10));
    }

    public Unit CreateGulyayGorod(int number, string tag)
    {
        return new GulyayGorodAdapter(
            name: MakeName("ГуляйГород", number, tag),
            defense: Vary(GulyayGorodAdapter.BaseDefense, 5),
            health: Vary(GulyayGorodAdapter.BaseHealth, 30));
    }

    // Фабричный метод: создаёт случайного юнита любого из 6 типов
    public Unit CreateRandom(int number, string tag)
    {
        return _rng.Next(6) switch
        {
            0 => CreateHeavyInfantry(number, tag),
            1 => CreateLightInfantry(number, tag),
            2 => CreateArcher(number, tag),
            3 => CreateHealer(number, tag),
            4 => CreateWizard(number, tag),
            _ => CreateGulyayGorod(number, tag)
        };
    }

    private string MakeName(string type, int number, string tag)
    {
        return string.IsNullOrEmpty(tag) ? $"{type}#{number}" : $"{tag}:{type}#{number}";
    }

    private int Vary(int baseValue, int variance)
    {
        return baseValue + _rng.Next(-variance, variance + 1);
    }
}
