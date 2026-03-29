using BattleGame.Interfaces;

namespace BattleGame.Units;

// Лёгкий пехотинец — высокий урон, но мало HP и защиты.
// Роль: дамагер, быстро убивает, но быстро гибнет.
// ICanBeHealed — может быть исцелён хилером.
// ICanBeCloned — может быть клонирован магом.
public class LightInfantry : Unit, ICanBeHealed, ICanBeCloned
{
    // Базовые характеристики — используются ArmyFactory как основа для рандомизации
    public const int BaseDamage = 30;
    public const int BaseDefense = 5;
    public const int BaseHealth = 80;

    // Пустой конструктор обязателен для десериализации из JSON
    public LightInfantry() { }

    public LightInfantry(string name, int damage, int defense, int health)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
    }

    // Клонирование: создаёт копию с текущими характеристиками
    public Unit Clone()
    {
        return new LightInfantry(Name + "*", Damage, Defense, Health) { MaxHealth = MaxHealth };
    }
}
