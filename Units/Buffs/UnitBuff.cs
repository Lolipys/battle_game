using System.Text.Json.Serialization;

namespace BattleGame.Units;

// Decorator: бафф сам является Unit'ом - снаружи неотличим
// Характеристики делегируются по цепочке Inner-юнитов
public abstract class UnitBuff : Unit
{
    // public — нужен для JSON-сериализации (System.Text.Json требует public)
    public Unit Inner { get; set; } = null!;

    // имя используется для проверки "не вешать дважды" и для логирования
    [JsonIgnore]
    public abstract string BuffName { get; }

    [JsonIgnore]
    public virtual int BonusDamage => 0;

    [JsonIgnore]
    public virtual int BonusDefense => 0;

    protected UnitBuff() { }

    protected UnitBuff(Unit inner)
    {
        Inner = inner;
        Name = inner.Name;
    }

    // все свойства делегируются Inner; сеттеры нужны для Healer.UseAbility (target.Health += healAmount)
    public override int Health
    {
        get => Inner?.Health ?? 0;
        set { if (Inner != null) Inner.Health = value; }
    }

    public override int MaxHealth
    {
        get => Inner?.MaxHealth ?? 0;
        set { if (Inner != null) Inner.MaxHealth = value; }
    }

    public override int Damage
    {
        get => (Inner?.Damage ?? 0) + BonusDamage;
        set { if (Inner != null) Inner.Damage = value - BonusDamage; }
    }

    public override int Defense
    {
        get => (Inner?.Defense ?? 0) + BonusDefense;
        set { if (Inner != null) Inner.Defense = value - BonusDefense; }
    }

    public override void TakeDamage(int damage)
    {
        Inner.TakeDamage(damage);
    }

    public Unit Strip() => Inner;

    public static bool HasBuff(Unit unit, string buffName)
    {
        Unit? current = unit;
        while (current is UnitBuff buff)
        {
            if (buff.BuffName == buffName) return true;
            current = buff.Inner;
        }
        return false;
    }

    public static Unit Unwrap(Unit unit)
    {
        while (unit is UnitBuff buff)
            unit = buff.Inner;
        return unit;
    }

    public static Unit StripBuff(Unit unit, string buffName, out bool stripped)
    {
        stripped = false;

        if (unit is UnitBuff topBuff && topBuff.BuffName == buffName)
        {
            stripped = true;
            return topBuff.Inner;
        }

        // нашли нужный бафф в глубине — пересобираем цепочку поверх него
        var stack = new Stack<UnitBuff>();
        Unit cursor = unit;
        while (cursor is UnitBuff b)
        {
            if (b.BuffName == buffName)
            {
                stripped = true;
                cursor = b.Inner;
                break;
            }
            stack.Push(b);
            cursor = b.Inner;
        }
        if (!stripped) return unit;

        while (stack.Count > 0)
        {
            var b = stack.Pop();
            b.Inner = cursor;
            cursor = b;
        }
        return cursor;
    }

    public override string ToString()
    {
        var buffs = new List<string>();
        Unit cursor = this;
        while (cursor is UnitBuff b)
        {
            buffs.Add(b.BuffName);
            cursor = b.Inner;
        }
        string buffTag = " +[" + string.Join(",", buffs) + "]";
        return $"{cursor.Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}]{buffTag}";
    }
}
