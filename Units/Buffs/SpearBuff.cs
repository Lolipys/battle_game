namespace BattleGame.Units;

// Копьё: +атака.
public class SpearBuff : UnitBuff
{
    public const string Tag = "Копьё";
    public const int Atk = 5;

    public override string BuffName => Tag;
    public override int BonusDamage => Atk;

    public SpearBuff() { }
    public SpearBuff(Unit inner) : base(inner) { }
}
