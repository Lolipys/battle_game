using MedievalRussia;

namespace BattleGame.Units;

// Adapter: оборачивает GulyayGorod из MedievalRussia.dll в нашу иерархию Unit
// Не атакует, не лечится, не клонируется
public class GulyayGorodAdapter : Unit
{
    public const int BaseDefense = 30;
    public const int BaseHealth = 200;

    private GulyayGorod _inner;

    public GulyayGorodAdapter()
    {
        _inner = new GulyayGorod(BaseHealth, BaseDefense);
    }

    public GulyayGorodAdapter(string name, int defense, int health)
    {
        Name = name;
        Damage = 0;
        Defense = defense;
        Health = health;
        MaxHealth = health;
        _inner = new GulyayGorod(health, defense);
    }

    // override скрывает Unit.Attack: GulyayGorod никогда не атакует
    public override int Attack(Unit target)
    {
        return 0;
    }

    // синхронизируем _inner, чтобы состояние DLL не рассинхронизировалось
    public override void TakeDamage(int damage)
    {
        Health = Math.Max(Health - damage, 0);
        _inner.ReduceHealth(damage);
    }

    public override string ToString()
    {
        return $"{Name} [HP: {Health}/{MaxHealth}, DEF: {Defense}, ATK: нет]";
    }
}
