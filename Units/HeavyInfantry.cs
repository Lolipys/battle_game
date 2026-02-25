namespace BattleGame.Units;

public class HeavyInfantry : Unit
{
    public const int BaseDamage = 20;
    public const int BaseDefense = 15;
    public const int BaseHealth = 150;

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
