using BattleGame.Units;

namespace BattleGame.Interfaces;

// Паттерн Observer.
// Наблюдатели подписываются на боевые события Battlefield (без C# event — классическая GoF-схема).
// Реализаций две:
//   DeathObserver  — реагирует на гибель юнита (логирует, ведёт счёт потерь).
//   DamageObserver — реагирует на полученный урон (отслеживает HP для правила ничьи).
public interface IBattleObserver
{
    // Юнит получил урон. damage = фактически снятые HP.
    void OnDamage(Unit target, Army army, int damage, int hpBefore);

    // Юнит погиб (HP <= 0).
    void OnDeath(Unit unit, Army army);

    // Сигнал начала нового хода (используется DamageObserver для подсчёта статичных ходов).
    void OnTurnStarted(int turnNumber);
}
