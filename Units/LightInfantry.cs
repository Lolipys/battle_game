namespace BattleGame.Units;

public class LightInfantry : Unit
{
    public const int BaseDamage = 30;
    public const int BaseDefense = 5;
    public const int BaseHealth = 80;

    public LightInfantry() { }

    public LightInfantry(string name, int damage, int defense, int health)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
    }
}
