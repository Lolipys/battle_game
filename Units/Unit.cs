using System.Text.Json.Serialization;

namespace BattleGame.Units;

[JsonDerivedType(typeof(HeavyInfantry), "heavy")]
[JsonDerivedType(typeof(LightInfantry), "light")]
[JsonDerivedType(typeof(Archer), "archer")]
public abstract class Unit
{
    public string Name { get; set; } = string.Empty;
    public int Damage { get; set; }
    public int Defense { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }

    [JsonIgnore]
    public decimal Price => Damage * 2m + Defense * 1.5m + MaxHealth;

    [JsonIgnore]
    public bool IsAlive => Health > 0;

    public int Attack(Unit target)
    {
        int actualDamage = Math.Max(Damage - target.Defense, 1);
        target.Health = Math.Max(target.Health - actualDamage, 0);
        return actualDamage;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}]";
    }
}
