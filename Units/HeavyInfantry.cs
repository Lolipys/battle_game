namespace BattleGame.Units;

// Тяжёлый пехотинец — высокая защита и HP, средний урон.
// Роль: танк, принимает удары на передовой.
public class HeavyInfantry : Unit
{
    // Базовые характеристики — используются ArmyFactory как основа для рандомизации
    public const int BaseDamage = 20;
    public const int BaseDefense = 15;
    public const int BaseHealth = 150;

    // Пустой конструктор обязателен для десериализации из JSON
    public HeavyInfantry() { }

    public HeavyInfantry(string name, int damage, int defense, int health)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
    }
}
