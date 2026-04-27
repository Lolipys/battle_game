using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.Services.Observers;

// Наблюдатель урона.
// Подписан на одно событие — OnDamage. Накапливает суммарный урон в текущем ходе.
// Используется для правила ничьи: если 10 ходов подряд никто не получает урон — игра в патовой ситуации.
// Это требование с лекции "upd по гуляй город".
public class DamageObserver : IBattleObserver
{
    public const int DrawAfterStaticTurns = 10;

    private int _damageThisTurn;
    private int _staticTurns;

    // True, если игра достигла патового состояния (10 ходов без урона).
    public bool IsDraw => _staticTurns >= DrawAfterStaticTurns;

    public int StaticTurns => _staticTurns;

    public void OnDamage(Unit target, Army army, int damage, int hpBefore)
    {
        if (damage > 0)
            _damageThisTurn += damage;
    }

    public void OnDeath(Unit unit, Army army) { }

    // В начале нового хода смотрим, был ли урон в прошлом ходу.
    // Если нет — увеличиваем счётчик статичных ходов; если был — сбрасываем.
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

    // Сброс счётчиков (используется при Undo, чтобы не накапливалась ничья).
    public void Reset()
    {
        _damageThisTurn = 0;
        _staticTurns = 0;
    }
}
