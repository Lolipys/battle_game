using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Observers;

// правило ничьи: 10 ходов без урона = IsDraw = true
public class DamageObserver : IBattleObserver
{
    public const int DrawAfterStaticTurns = 10;

    private int _damageThisTurn;
    private int _staticTurns;

    public bool IsDraw => _staticTurns >= DrawAfterStaticTurns;

    public int StaticTurns => _staticTurns;

    public void OnDamage(Unit target, Army army, int damage, int hpBefore)
    {
        if (damage > 0)
            _damageThisTurn += damage;
    }

    public void OnDeath(Unit unit, Army army) { }

    public void OnTurnStarted(int turnNumber)
    {
        if (turnNumber <= 1)
        {
            _damageThisTurn = 0;
            return;
        }

        if (_damageThisTurn == 0)
            _staticTurns++;
        else
            _staticTurns = 0;

        _damageThisTurn = 0;
    }

    // вызывается при Undo, чтобы ничья не накапливалась из откатанных ходов
    public void Reset()
    {
        _damageThisTurn = 0;
        _staticTurns = 0;
    }
}
