using BattleGame.Interfaces;

namespace BattleGame.Units;

// Маг — с маленькой вероятностью клонирует союзника и ставит его перед собой.
// Реализует ISpecialAbility. НЕ реализует ICanBeHealed, ICanBeCloned.
// Клонирует только юнитов с ICanBeCloned (Light, Archer) в пределах Range.
public class Wizard : Unit, ISpecialAbility
{
    public const int BaseDamage = 6;
    public const int BaseDefense = 2;
    public const int BaseHealth = 45;
    public const int BaseRange = 3;
    public const int BasePower = 0;        // Power не используется (клон, а не урон)
    public const int BaseCloneChance = 30; // вероятность клонирования в процентах

    // Радиус: маг на позиции N может клонировать юнитов в пределах Range позиций от себя
    public int Range { get; set; }
    public int Power { get; set; }

    // Шанс клонирования (0–100%)
    public int CloneChance { get; set; }

    public Wizard() { }

    public Wizard(string name, int damage, int defense, int health, int range, int cloneChance)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
        Range = range;
        Power = 0;
        CloneChance = cloneChance;
    }

    // UseAbility у мага не наносит урон — клонирование обрабатывается в Battlefield.
    // Этот метод нужен только для совместимости с ISpecialAbility.
    public int UseAbility(Unit target)
    {
        return 0;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, CLONE: {CloneChance}%]";
    }
}
