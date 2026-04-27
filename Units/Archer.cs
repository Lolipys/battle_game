using BattleGame.Interfaces;

namespace BattleGame.Units;

// Лучник — слабый в ближнем бою, но стреляет с дальних позиций.
// Реализует ISpecialAbility: стрелы пробивают 50% защиты цели.
// ICanBeHealed — может быть исцелён хилером.
// ICanBeCloned — может быть клонирован магом.
public class Archer : Unit, ISpecialAbility, ICanBeHealed, ICanBeCloned
{
    // Базовые характеристики — используются ArmyFactory как основа для рандомизации
    public const int BaseDamage = 8;
    public const int BaseDefense = 3;
    public const int BaseHealth = 60;
    public const int BaseRange = 4;
    public const int BasePower = 25;

    // Дальность стрельбы: юнит на позиции N может стрелять только если Range >= N
    public int Range { get; set; }

    // Сила стрелы: базовый урон до вычета защиты
    public int Power { get; set; }

    // Пустой конструктор обязателен для десериализации из JSON
    public Archer() { }

    public Archer(string name, int damage, int defense, int health, int range, int power)
    {
        Name = name;
        Damage = damage;
        Defense = defense;
        Health = health;
        MaxHealth = health;
        Range = range;
        Power = power;
    }

    // Выстрел по цели. Стрела игнорирует половину защиты (Defense / 2).
    // Это делает лучников эффективными против тяжёлых пехотинцев.
    public int UseAbility(Unit target)
    {
        int arrowDamage = Math.Max(Power - target.Defense / 2, 1);
        target.TakeDamage(arrowDamage);
        return arrowDamage;
    }

    // Клонирование: создаёт копию с текущими характеристиками
    public Unit Clone()
    {
        return new Archer(Name + "*", Damage, Defense, Health, Range, Power) { MaxHealth = MaxHealth };
    }

    // Расширенный вывод: добавлены RNG (дальность) и PWR (сила стрелы)
    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}, RNG: {Range}, PWR: {Power}]";
    }
}
