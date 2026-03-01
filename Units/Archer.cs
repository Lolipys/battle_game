using BattleGame.Interfaces;

namespace BattleGame.Units;

public class Archer : Unit, ISpecialAbility
{
    public const int BaseDamage = 8;
    public const int BaseDefense = 3;
    public const int BaseHealth = 60;
    public const int BaseRange = 4;
    public const int BasePower = 25;

    public int Range { get; set; }
    public int Power { get; set; }

    public Archer() { }

    public Archer(string name, int damage, int defense, int health, int range, int power)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
        Range = range;
        Power = power;
    }

    public int UseAbility(Unit target)
    {
        int arrowDamage = Math.Max(Power - target.Defense / 2, 1);
        target.Health = Math.Max(target.Health - arrowDamage, 0);
        return arrowDamage;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, PWR: {Power}]";
    }
}
