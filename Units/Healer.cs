using BattleGame.Interfaces;

namespace BattleGame.Units;

// Роль: поддержка. Лечит ICanBeHealed-союзников в радиусе Range
// НЕ реализует ICanBeCloned — маг не клонирует хилеров
public class Healer : Unit, ISpecialAbility, ICanBeHealed
{
    public const int BaseDamage = 5;
    public const int BaseDefense = 2;
    public const int BaseHealth = 50;
    public const int BaseRange = 3;
    public const int BasePower = 20;

    // позиция N лечит, если Range >= N (та же семантика, что у Archer)
    public int Range { get; set; }
    public int Power { get; set; }

    public Healer() { }

    public Healer(string name, int damage, int defense, int health, int range, int power)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
        Range = range;
        Power = power;
    }

    // HP не превысит MaxHealth; возвращает реально восстановленное количество
    public int UseAbility(Unit target)
    {
        int healAmount = Math.Min(Power, target.MaxHealth - target.Health);
        target.Health += healAmount;
        return healAmount;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, HEAL: {Power}]";
    }
}
