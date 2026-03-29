using BattleGame.Interfaces;

namespace BattleGame.Units;

// Хилер — лечит союзников в радиусе действия.
// Реализует ISpecialAbility: лечение = «атака с отрицательным уроном».
// ICanBeHealed — другой хилер может лечить этого.
// НЕ реализует ICanBeCloned — маг не может клонировать хилера.
public class Healer : Unit, ISpecialAbility, ICanBeHealed
{
    public const int BaseDamage = 5;
    public const int BaseDefense = 2;
    public const int BaseHealth = 50;
    public const int BaseRange = 3;
    public const int BasePower = 20;  // сила лечения

    // Дальность: хилер на позиции N лечит, если Range >= N
    public int Range { get; set; }

    // Сила лечения (аналог Power у лучника, но восстанавливает HP)
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

    // UseAbility у хилера вызывается с союзником, не с врагом.
    // Лечение реализовано как «атака с отрицательным уроном»:
    // HP цели увеличивается, но не выше MaxHealth.
    // Возвращает количество восстановленного HP.
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
