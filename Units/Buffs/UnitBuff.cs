using System.Text.Json.Serialization;

namespace BattleGame.Units;

// Паттерн Decorator: базовый класс для баффов, оборачивающих другого юнита.
// Бафф сам является Unit'ом — снаружи неотличим. Inner-юнит сериализуется как ссылка.
// Все характеристики (Health, MaxHealth, Damage, Defense) делегируются Inner — полиморфно через цепочку.
public abstract class UnitBuff : Unit
{
    // Внутренний юнит (или другой бафф ниже по цепочке). Public для JSON-сериализации.
    public Unit Inner { get; set; } = null!;

    // Имя баффа для отображения и проверки "не вешать дважды"
    [JsonIgnore]
    public abstract string BuffName { get; }

    // Бонус, который этот бафф добавляет (переопределяется наследниками)
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

    // Делегируем всё во внутренний юнит — цепочка декораторов всегда видит актуальные значения.
    // Сеттеры на Health/MaxHealth используются Healer.UseAbility, поэтому тоже делегируем.
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

    // Урон проходит во внутренний юнит (полиморфно — следующий бафф или сам Unit)
    public override void TakeDamage(int damage)
    {
        Inner.TakeDamage(damage);
    }

    // Снять этот бафф — вернуть Inner. Используется при "сбитии" баффа в бою.
    public Unit Strip() => Inner;

    // Проверка: висит ли уже бафф с таким именем где-то в цепочке
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

    // Развернуть всю цепочку и достать самого "глубокого" носителя
    public static Unit Unwrap(Unit unit)
    {
        while (unit is UnitBuff buff)
            unit = buff.Inner;
        return unit;
    }

    // Снять верхний бафф указанного типа из цепочки. Возвращает (возможно изменённого) юнита.
    public static Unit StripBuff(Unit unit, string buffName, out bool stripped)
    {
        stripped = false;

        // Бафф на самом верху — просто снимаем
        if (unit is UnitBuff topBuff && topBuff.BuffName == buffName)
        {
            stripped = true;
            return topBuff.Inner;
        }

        // Иначе ищем глубже и пересобираем цепочку
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
        // Собираем все баффы из цепочки в один список
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
