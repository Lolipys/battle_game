using MedievalRussia;

namespace BattleGame.Units;

// Адаптер (паттерн Adapter) для внешнего класса GulyayGorod из MedievalRussia.dll.
// Оборачивает чужой API в наш Unit. Не атакует, не лечится, не клонируется.
// НЕ реализует ICanBeHealed, ICanBeCloned, ISpecialAbility.
public class GulyayGorodAdapter : Unit
{
    public const int BaseDefense = 30;
    public const int BaseHealth = 200;

    // Внутренний объект из внешней DLL
    private GulyayGorod _inner;

    public GulyayGorodAdapter()
    {
        _inner = new GulyayGorod(BaseHealth, BaseDefense);
    }

    public GulyayGorodAdapter(string name, int defense, int health)
    {
        Name = name;
        Damage = 0;  // не атакует
        Defense = defense;
        Health = health;
        MaxHealth = health;
        _inner = new GulyayGorod(health, defense);
    }

    // Переопределяем атаку: Гуляй-город не атакует, всегда 0 урона
    public new int Attack(Unit target)
    {
        return 0;
    }

    // Синхронизация: при получении урона обновляем и внутренний объект
    public void TakeDamage(int damage)
    {
        int actualDamage = Math.Max(damage - Defense, 1);
        Health = Math.Max(Health - actualDamage, 0);
        _inner.ReduceHealth(actualDamage);
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, DEF: {Defense}, ATK: нет]";
    }
}
