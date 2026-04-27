namespace BattleGame.Units;

// Щит: +защита.
public class ShieldBuff : UnitBuff
{
    public const string Tag = "Щит";
    public const int Def = 5;

    public override string BuffName => Tag;
    public override int BonusDefense => Def;

    public ShieldBuff() { }
    public ShieldBuff(Unit inner) : base(inner) { }
}
