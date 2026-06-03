using BattleGame.Interfaces;

namespace BattleGame.Units;

// Роль: дамагер + оруженосец (надевает Decorator-баффы на соседних HeavyInfantry)
// ICanBeHealed, ICanBeCloned — участвует в лечении и клонировании
public class LightInfantry : Unit, ISpecialAbility, ICanBeHealed, ICanBeCloned
{
    public const int BaseDamage = 30;
    public const int BaseDefense = 5;
    public const int BaseHealth = 80;
    public const int BaseRange = 1;       // только сосед слева/справа
    public const int BaseBuffChance = 60; // % шанс надеть бафф за ход

    public int Range { get; set; } = BaseRange;
    public int BuffChance { get; set; } = BaseBuffChance;

    // Power нужен для ISpecialAbility, но оруженосец не наносит урона
    public int Power { get; set; } = 0;

    public LightInfantry() { }

    public LightInfantry(string name, int damage, int defense, int health)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
    }

    // реальная логика в Battlefield.ProcessSquire (нужен доступ к соседям армии)
    public int UseAbility(Unit target) => 0;

    public Unit Clone()
    {
        return new LightInfantry(Name + "*", Damage, Defense, Health) { MaxHealth = MaxHealth };
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, оруженосец {BuffChance}%]";
    }
}
