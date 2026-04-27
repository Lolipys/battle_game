namespace BattleGame.Units;

// Шлем: +защита (меньше, чем у щита).
public class HelmetBuff : UnitBuff
{
    public const string Tag = "Шлем";
    public const int Def = 3;

    public override string BuffName => Tag;
    public override int BonusDefense => Def;

    public HelmetBuff() { }
    public HelmetBuff(Unit inner) : base(inner) { }
}
