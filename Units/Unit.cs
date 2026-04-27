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
[JsonDerivedType(typeof(HorseBuff), "horse")]
[JsonDerivedType(typeof(SpearBuff), "spear")]
[JsonDerivedType(typeof(ShieldBuff), "shield")]
[JsonDerivedType(typeof(HelmetBuff), "helmet")]
public abstract class Unit
{
    public string Name { get; set; } = string.Empty;

    // virtual — декораторы (баффы) могут переопределить, чтобы делегировать во внутренний юнит
    public virtual int Damage { get; set; }
    public virtual int Defense { get; set; }
    public virtual int Health { get; set; }
    public virtual int MaxHealth { get; set; }

    // Вычисляемые свойства — не сохраняются в JSON, т.к. зависят от других полей
    [JsonIgnore]
    public int Price => Damage * 2 + (int)Math.Ceiling(Defense * 1.5) + MaxHealth;

    [JsonIgnore]
    public bool IsAlive => Health > 0;

    // Атака цели. Урон = своя атака - защита цели (минимум 1).
    // HP цели не опускается ниже 0.
    public virtual int Attack(Unit target)
    {
        int actualDamage = Math.Max(Damage - target.Defense, 1);
        target.TakeDamage(actualDamage);
        return actualDamage;
    }

    // Получение урона. virtual — декораторы могут перехватить.
    public virtual void TakeDamage(int damage)
    {
        Health = Math.Max(Health - damage, 0);
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}]";
    }
}
