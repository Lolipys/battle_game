using BattleGame.Interfaces;

namespace BattleGame.Units;

// Лёгкий пехотинец — высокий урон, но мало HP и защиты.
// Демо 3: получает спецспособность "оруженосец" — может надевать баффы (паттерн Decorator)
// на соседнего тяжёлого пехотинца. Range = 1 (только сосед слева/справа).
// ICanBeHealed — может быть исцелён хилером.
// ICanBeCloned — может быть клонирован магом.
public class LightInfantry : Unit, ISpecialAbility, ICanBeHealed, ICanBeCloned
{
    public const int BaseDamage = 30;
    public const int BaseDefense = 5;
    public const int BaseHealth = 80;
    public const int BaseRange = 1;       // только соседний слот
    public const int BaseBuffChance = 60; // % шанс надеть бафф, если есть тяжёлый рядом

    // Range и BuffChance — у лёгкого пехотинца отвечают за оруженосец-способность.
    public int Range { get; set; } = BaseRange;
    public int BuffChance { get; set; } = BaseBuffChance;

    // Power у лёгкого не используется (требование интерфейса)
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

    // UseAbility у лёгкого пехотинца не наносит урона напрямую.
    // Реальная логика "надеть бафф" обрабатывается стратегией боя (нужен доступ к соседям армии).
    public int UseAbility(Unit target) => 0;

    // Клонирование: создаёт копию с текущими характеристиками
    public Unit Clone()
    {
        return new LightInfantry(Name + "*", Damage, Defense, Health) { MaxHealth = MaxHealth };
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, оруженосец {BuffChance}%]";
    }
}
