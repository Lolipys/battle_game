using System.Text.Json.Serialization;

namespace BattleGame.Units;

// Базовый абстрактный класс для всех юнитов.
// Атрибуты JsonDerivedType регистрируют наследников для полиморфной JSON-сериализации.
[JsonDerivedType(typeof(HeavyInfantry), "heavy")]
[JsonDerivedType(typeof(LightInfantry), "light")]
[JsonDerivedType(typeof(Archer), "archer")]
[JsonDerivedType(typeof(Healer), "healer")]
[JsonDerivedType(typeof(Wizard), "wizard")]
[JsonDerivedType(typeof(GulyayGorodAdapter), "gulyay")]
[JsonDerivedType(typeof(UnitProxy), "proxy")]
public abstract class Unit
{
    public string Name { get; set; } = string.Empty;
    public int Damage { get; set; }
    public int Defense { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }

    // Вычисляемые свойства — не сохраняются в JSON, т.к. зависят от других полей
    [JsonIgnore]
    public decimal Price => Damage * 2m + Defense * 1.5m + MaxHealth;

    [JsonIgnore]
    public bool IsAlive => Health > 0;

    // Атака цели. Урон = своя атака - защита цели (минимум 1, чтобы бой не зацикливался).
    // Если у цели есть щит (UnitProxy) — сначала урон поглощается щитом.
    // HP цели не опускается ниже 0.
    public int Attack(Unit target)
    {
        int actualDamage = Math.Max(Damage - target.Defense, 1);

        // Паттерн Proxy: щит перехватывает часть урона
        if (target is UnitProxy proxy && proxy.Shield > 0)
        {
            int absorbed = proxy.AbsorbDamage(actualDamage);
            int remaining = actualDamage - absorbed;
            target.Health = Math.Max(target.Health - remaining, 0);
        }
        else
        {
            target.Health = Math.Max(target.Health - actualDamage, 0);
        }

        return actualDamage;
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}]";
    }
}
