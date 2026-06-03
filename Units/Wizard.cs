using BattleGame.Interfaces;

namespace BattleGame.Units;

// Роль: поддержка. Клонирует ICanBeCloned-союзников, вставляя клона перед собой
// НЕ реализует ICanBeHealed, ICanBeCloned
public class Wizard : Unit, ISpecialAbility
{
    public const int BaseDamage = 6;
    public const int BaseDefense = 2;
    public const int BaseHealth = 45;
    public const int BaseRange = 3;
    public const int BasePower = 0;
    public const int BaseCloneChance = 30;

    public int Range { get; set; }
    public int Power { get; set; }
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

    // клонирование обрабатывается в Battlefield.ProcessWizard; метод нужен для ISpecialAbility
    public int UseAbility(Unit target)
    {
        return 0;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, CLONE: {CloneChance}%]";
    }
}
