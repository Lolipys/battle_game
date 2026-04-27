namespace BattleGame.Units;

// Конь: +атака и +защита. Самый сильный бафф.
public class HorseBuff : UnitBuff
{
    public const string Tag = "Конь";
    public const int Atk = 6;
    public const int Def = 4;

    public override string BuffName => Tag;
    public override int BonusDamage => Atk;
    public override int BonusDefense => Def;

    public HorseBuff() { }
    public HorseBuff(Unit inner) : base(inner) { }
}
