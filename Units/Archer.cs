using BattleGame.Interfaces;

namespace BattleGame.Units;

// Роль: дальний урон. Стрела игнорирует 50% защиты цели
// ICanBeHealed, ICanBeCloned — лечится хилером, клонируется магом
public class Archer : Unit, ISpecialAbility, ICanBeHealed, ICanBeCloned
{
    public const int BaseDamage = 8;
    public const int BaseDefense = 3;
    public const int BaseHealth = 60;
    public const int BaseRange = 4;
    public const int BasePower = 25;

    // позиция N стреляет только если Range >= N
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

    // Defense / 2 игнорируется — эффективно против танков с высокой защитой
    public int UseAbility(Unit target)
    {
        int arrowDamage = Math.Max(Power - target.Defense / 2, 1);
        target.TakeDamage(arrowDamage);
        return arrowDamage;
    }

    public Unit Clone()
    {
        return new Archer(Name + "*", Damage, Defense, Health, Range, Power) { MaxHealth = MaxHealth };
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, PWR: {Power}]";
    }
}
