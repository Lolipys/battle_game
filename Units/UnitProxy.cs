namespace BattleGame.Units;

// Паттерн Proxy: оборач��вает любого юнита, добавляя щит (барьер).
// Пока щит > 0, весь урон идёт в щит. Когда щит разрушен — урон идёт юниту напрямую.
// Внешне выглядит как обычный Unit (тот же интерфейс).
public class UnitProxy : Unit
{
    public const int BaseShield = 40;

    // Количество очков щита (барьера)
    public int Shield { get; set; }

    // Ссылка на реального юнита (для логирования и понимания, кто под щитом)
    public string InnerUnitType { get; set; } = string.Empty;

    public UnitProxy() { }

    // Создаём прокси вокруг существующего юнита: копируем все характеристики + добавляем щит
    public UnitProxy(Unit inner, int shield)
    {
        Name = inner.Name + " [щит]";
        Damage = inner.Damage;
        Defense = inner.Defense;
        Health = inner.Health;
        MaxHealth = inner.MaxHealth;
        Shield = shield;
        InnerUnitType = inner.GetType().Name;
    }

    // Перехват урона: сначала щит поглощает, остаток идёт в HP
    public int AbsorbDamage(int incomingDamage)
    {
        if (Shield <= 0)
            return 0;

        int absorbed = Math.Min(Shield, incomingDamage);
        Shield -= absorbed;
        return absorbed;
    }

    public override string ToString()
    {
        string shieldText = Shield > 0 ? $", SHIELD: {Shield}" : ", щит разрушен";
        return $"{Name} [HP: {Health}/{MaxHealth}, ATK: {Damage}, DEF: {Defense}{shieldText}]";
    }
}
