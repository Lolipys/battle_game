using System.Text.Json.Serialization;
using BattleGame.Interfaces;
using BattleGame.Services;
using BattleGame.Units;

namespace BattleGame;

// Ядро игры — управляет боем между двумя армиями.
// Каждый ход: ближний бой → спецспособности (атака/лечение/клон) → очистка поля.
// Логирование делегируется IBattleLogger (принцип D — Dependency Inversion).
public class Battlefield
{
    private static readonly Random Rng = new();

    public Army Army1 { get; set; } = new();
    public Army Army2 { get; set; } = new();
    public int TurnNumber { get; set; }

    // Логгер не сериализуется — это сервис, а не данные. Назначается заново после загрузки.
    [JsonIgnore]
    public IBattleLogger Logger { get; set; } = new BattleLog();

    [JsonIgnore]
    public bool IsGameOver => Army1.IsDefeated || Army2.IsDefeated;

    // Пустой конструктор — для десериализации из JSON
    public Battlefield() { }

    // Основной конструктор — внедрение зависимости логгера (Dependency Injection)
    public Battlefield(IBattleLogger logger)
    {
        Logger = logger;
    }

    // Один игровой ход: ближний бой → спецспособности → очистка → проверка победы
    public void MakeTurn()
    {
        if (IsGameOver)
        {
            Logger.GameOver(GetWinner());
            return;
        }

        TurnNumber++;
        Logger.TurnHeader(TurnNumber);

        MeleePhase();
        SpecialAbilityPhase();
        CleanupPhase();

        Logger.PrintArmyStatus(Army1, Army2);

        if (IsGameOver)
            Logger.GameOver(GetWinner());
    }

    // Играть до конца — автоматически выполняет ходы до победы одной из сторон
    public void PlayToEnd()
    {
        while (!IsGameOver)
        {
            MakeTurn();
        }
    }

    public void PrintArmyStatus()
    {
        Logger.PrintArmyStatus(Army1, Army2);
    }

    // Фаза 1: Ближний бой.
    // Первые юниты обеих армий бьют друг друга.
    // Если защитник гибнет от атаки — ответного удара НЕТ.
    // GulyayGorod не атакует (Damage = 0, урон всегда = 1 из-за min).
    private void MeleePhase()
    {
        Logger.PhaseHeader("Ближний бой");

        var attacker = Army1.FirstUnit;
        var defender = Army2.FirstUnit;

        if (attacker == null || defender == null)
            return;

        // Атака первого юнита Армии 1
        int shieldBefore = GetShield(defender);
        int damage = attacker.Attack(defender);
        Logger.MeleeAttack(attacker, defender, damage);
        LogShieldChange(defender, shieldBefore);

        // Если защитник убит — ответного удара не будет
        if (!defender.IsAlive)
        {
            Logger.UnitDied(defender, Army2.Name);
            return;
        }

        // Ответный удар защитника (только если выжил)
        shieldBefore = GetShield(attacker);
        int counterDamage = defender.Attack(attacker);
        Logger.CounterAttack(defender, attacker, counterDamage);
        LogShieldChange(attacker, shieldBefore);

        if (!attacker.IsAlive)
            Logger.UnitDied(attacker, Army1.Name);
    }

    // Фаза 2: Спецспособности.
    // Лучники стреляют во врагов, хилеры лечат союзников, маги клонируют.
    private void SpecialAbilityPhase()
    {
        bool hasAbilities = HasSpecialAbilityUnits(Army1) || HasSpecialAbilityUnits(Army2);
        if (!hasAbilities)
            return;

        Logger.PhaseHeader("Специальные способности");

        ProcessArmyAbilities(Army1, Army2);
        ProcessArmyAbilities(Army2, Army1);
    }

    // Проверяет, есть ли в армии юниты со спецспособностью (начиная с позиции 2)
    private static bool HasSpecialAbilityUnits(Army army)
    {
        for (int i = 1; i < army.Units.Count; i++)
        {
            if (army.Units[i] is ISpecialAbility)
                return true;
        }
        return false;
    }

    // Обработка спецспособностей одной армии.
    // Хилер → лечит рандомного союзника с ICanBeHealed (не себя).
    // Маг → с вероятностью клонирует союзника с ICanBeCloned.
    // Лучник → стреляет в первого живого врага.
    private void ProcessArmyAbilities(Army allies, Army enemies)
    {
        // Используем Count, т.к. маг может добавить клонов в список
        for (int i = 1; i < allies.Units.Count; i++)
        {
            if (!allies.Units[i].IsAlive)
                continue;

            if (allies.Units[i] is not ISpecialAbility ability)
                continue;

            int position = i + 1;

            // Хилер — лечит союзников (проверка дальности внутри ProcessHealer)
            if (allies.Units[i] is Healer healer)
            {
                ProcessHealer(healer, allies, i);
                continue;
            }

            // Маг — клонирует союзников (проверка дальности внутри ProcessWizard)
            if (allies.Units[i] is Wizard wizard)
            {
                ProcessWizard(wizard, allies, i);
                continue;
            }

            // Лучник и остальные — атакуют врага, проверяем дальность до передовой
            if (ability.Range < position)
            {
                Logger.AbilityOutOfRange(allies.Units[i], position, ability.Range);
                continue;
            }

            var target = enemies.FirstAliveUnit;
            if (target == null)
                break;

            int shieldBefore = GetShield(target);
            int damage = ability.UseAbility(target);
            Logger.AbilityUsed(allies.Units[i], target, damage, position, ability.Range);
            LogShieldChange(target, shieldBefore);

            if (!target.IsAlive)
                Logger.UnitDied(target, enemies.Name);
        }
    }

    // Хилер: выбирает рандомного союзника с ICanBeHealed (не себя, HP < MaxHP)
    private void ProcessHealer(Healer healer, Army allies, int healerIndex)
    {
        int position = healerIndex + 1;

        // Собираем кандидатов: живые, ICanBeHealed, не сам хилер, HP < MaxHP, в пределах Range
        var candidates = new List<Unit>();
        for (int j = 0; j < allies.Units.Count; j++)
        {
            if (j == healerIndex) continue;                     // не лечит себя
            if (!allies.Units[j].IsAlive) continue;
            if (allies.Units[j] is not ICanBeHealed) continue;  // только ICanBeHealed
            if (allies.Units[j].Health >= allies.Units[j].MaxHealth) continue; // уже полное HP
            candidates.Add(allies.Units[j]);
        }

        if (candidates.Count == 0)
            return;

        // Рандомный выбор цели из подходящих
        var target = candidates[Rng.Next(candidates.Count)];
        int healAmount = healer.UseAbility(target);

        if (healAmount > 0)
            Logger.HealUsed(healer, target, healAmount, position, healer.Range);
    }

    // Маг: с CloneChance% вероятностью клонирует союзника с ICanBeCloned в пределах Range
    private void ProcessWizard(Wizard wizard, Army allies, int wizardIndex)
    {
        int position = wizardIndex + 1;

        // Проверяем, сработало ли клонирование
        if (Rng.Next(100) >= wizard.CloneChance)
        {
            Logger.CloneFailed(wizard, position);
            return;
        }

        // Собираем кандидатов для клонирования: живые, ICanBeCloned, в пределах Range
        var candidates = new List<(Unit unit, int index)>();
        for (int j = 0; j < allies.Units.Count; j++)
        {
            if (!allies.Units[j].IsAlive) continue;
            if (allies.Units[j] is not ICanBeCloned) continue;

            // Проверяем дальность: расстояние между позициями <= Range
            int distance = Math.Abs(j - wizardIndex);
            if (distance > wizard.Range) continue;

            candidates.Add((allies.Units[j], j));
        }

        if (candidates.Count == 0)
        {
            Logger.CloneFailed(wizard, position);
            return;
        }

        // Рандомный выбор цели
        var (original, _) = candidates[Rng.Next(candidates.Count)];
        var clone = ((ICanBeCloned)original).Clone();

        // Вставляем клона перед магом
        allies.Units.Insert(wizardIndex, clone);

        Logger.CloneUsed(wizard, original, clone, position);
    }

    // Фаза 3: Очистка поля.
    // Удаляет мёртвых юнитов, остальные сдвигаются к передовой.
    private void CleanupPhase()
    {
        int deadArmy1 = Army1.Units.Count(u => !u.IsAlive);
        int deadArmy2 = Army2.Units.Count(u => !u.IsAlive);

        Army1.RemoveDeadUnits();
        Army2.RemoveDeadUnits();

        if (deadArmy1 + deadArmy2 > 0)
        {
            Logger.PhaseHeader("Очистка поля");
            Logger.CleanupResult(deadArmy1, deadArmy2, Army1, Army2);
        }
    }

    // Логирование щита: вызывать с Shield ДО и ПОСЛЕ атаки
    private int GetShield(Unit target)
    {
        return target is UnitProxy proxy ? proxy.Shield : -1;
    }

    private void LogShieldChange(Unit target, int shieldBefore)
    {
        if (shieldBefore < 0) return;  // не прокси
        if (target is not UnitProxy proxy) return;

        int absorbed = shieldBefore - proxy.Shield;
        if (absorbed > 0)
        {
            Logger.ShieldAbsorbed(target, absorbed, proxy.Shield);
            if (proxy.Shield <= 0)
                Logger.ShieldDestroyed(target);
        }
    }

    // Определяет победителя. null = ничья (обе армии уничтожены одновременно)
    private Army? GetWinner()
    {
        if (Army1.IsDefeated && Army2.IsDefeated)
            return null;
        if (Army2.IsDefeated)
            return Army1;
        if (Army1.IsDefeated)
            return Army2;
        return null;
    }
}
