namespace BattleGame.Interfaces;

public interface ISpecialAbility
{
    int Range { get; }
    int Power { get; }
    int UseAbility(Units.Unit target);
}
