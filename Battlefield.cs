using System.Text.Json;
using System.Text.Json.Serialization;
using BattleGame.Interfaces;
using BattleGame.Services;
using BattleGame.Services.Observers;
using BattleGame.Services.Strategies;
using BattleGame.Units;

namespace BattleGame;

// Ядро игры — управляет боем между двумя армиями.
// Демо 3:
//   • Strategy   — фаза ближнего боя делегируется IBattleStrategy (3 режима, смена в любой момент).
//   • Observer   — DeathObserver, DamageObserver подписаны на события урона/смерти/начала хода.
//   • Decorator  — лёгкие пехотинцы (оруженосцы) надевают баффы на соседних тяжёлых.
//   • Ничья      — DamageObserver сигнализирует пат через 10 ходов без урона.
public class Battlefield
{
    private static readonly Random Rng = new();

    public Army Army1 { get; set; } = new();
    public Army Army2 { get; set; } = new();
    public int TurnNumber { get; set; }

    // Имя текущей стратегии (сериализуется), реальный объект — резолвится в SetStrategy.
    public string StrategyName { get; set; } = "";

    [JsonIgnore]
    public IBattleStrategy Strategy { get; private set; } = new BridgeStrategy();

    [JsonIgnore]
    public IBattleLogger Logger { get; set; } = new BattleLog();

    // Наблюдатели не сериализуются — они сервисы, переподписываются после загрузки
    [JsonIgnore]
    private readonly List<IBattleObserver> _observers = new();

    [JsonIgnore]
    public DamageObserver? DamageObserver { get; private set; }

    [JsonIgnore]
    public bool IsGameOver => Army1.IsDefeated || Army2.IsDefeated || (DamageObserver?.IsDraw ?? false);

    public Battlefield() { }

    public Battlefield(IBattleLogger logger)
    {
        Logger = logger;
        SetStrategy(new BridgeStrategy());
    }

    // --- Подписка наблюдателей (паттерн Observer) ---
    public void Subscribe(IBattleObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        if (observer is DamageObserver d)
            DamageObserver = d;
    }

    public void Unsubscribe(IBattleObserver observer)
    {
        _observers.Remove(observer);
        if (observer is DamageObserver) DamageObserver = null;
    }

    public void NotifyDamage(Unit unit, Army army, int damage, int hpBefore)
    {
        foreach (var o in _observers) o.OnDamage(unit, army, damage, hpBefore);
    }

    public void NotifyDeath(Unit unit, Army army)
    {
        foreach (var o in _observers) o.OnDeath(unit, army);
    }

    private void NotifyTurnStarted(int turn)
    {
        foreach (var o in _observers) o.OnTurnStarted(turn);
    }

    // --- Stratergy: смена в любой момент игры ---
    public void SetStrategy(IBattleStrategy strategy)
    {
        Strategy = strategy;
        StrategyName = strategy.Name;
        Logger?.StrategyChanged(strategy.Name);
    }

    // Один игровой ход: ближний бой (стратегия) → спецспособности → очистка → проверка победы
    public void MakeTurn()
    {
        if (IsGameOver)
        {
            AnnounceEnd();
            return;
        }

        TurnNumber++;
        Logger.TurnHeader(TurnNumber);
        NotifyTurnStarted(TurnNumber);

        Strategy.ExecuteMelee(this);
        SpecialAbilityPhase();
        CleanupPhase();

        Logger.PrintArmyStatus(Army1, Army2);

        if (IsGameOver)
            AnnounceEnd();
    }

    public void PlayToEnd()
    {
        while (!IsGameOver)
            MakeTurn();
    }

    public void PrintArmyStatus() => Logger.PrintArmyStatus(Army1, Army2);

    private void AnnounceEnd()
    {
        if (DamageObserver?.IsDraw == true && !Army1.IsDefeated && !Army2.IsDefeated)
        {
            Logger.Draw($"10 ходов без изменений HP — пат.");
            return;
        }
        Logger.GameOver(GetWinner());
    }

    // --- Фаза 2: Спецспособности ---
    // Хилер → лечит, Маг → клонирует, Лучник → стреляет, Лёгкий → оруженосец (декоратор).
    private void SpecialAbilityPhase()
    {
        bool any = HasAbilityUsers(Army1) || HasAbilityUsers(Army2);
        if (!any) return;

        Logger.PhaseHeader("Специальные способности");

        ProcessArmyAbilities(Army1, Army2);
        ProcessArmyAbilities(Army2, Army1);
    }

    private static bool HasAbilityUsers(Army army)
    {
        for (int i = 0; i < army.Units.Count; i++)
        {
            // На фронте обычно бьют в melee, но если фронта нет (армия пустая) — пропускаем
            if (army.Units[i] is ISpecialAbility) return true;
        }
        return false;
    }

    private void ProcessArmyAbilities(Army allies, Army enemies)
    {
        for (int i = 0; i < allies.Units.Count; i++)
        {
            var u = allies.Units[i];
            if (!u.IsAlive) continue;
            if (u is not ISpecialAbility ability) continue;

            int position = i + 1;

            // Лёгкий пехотинец-оруженосец: надевает бафф на соседнего тяжёлого
            if (UnitBuff.Unwrap(u) is LightInfantry light)
            {
                ProcessSquire(light, allies, i);
                continue;
            }

            // Хилер
            if (UnitBuff.Unwrap(u) is Healer healer)
            {
                ProcessHealer(healer, allies, i);
                continue;
            }

            // Маг
            if (UnitBuff.Unwrap(u) is Wizard wizard)
            {
                ProcessWizard(wizard, allies, i);
                continue;
            }

            // Стрелок: пропускаем фронт (он бил в melee), проверяем дальность
            if (i == 0) continue;
            if (ability.Range < position)
            {
                Logger.AbilityOutOfRange(u, position, ability.Range);
                continue;
            }

            var target = enemies.FirstAliveUnit;
            if (target == null) break;

            int hpBefore = target.Health;
            int dmg = ability.UseAbility(target);
            Logger.AbilityUsed(u, target, dmg, position, ability.Range, hpBefore);
            NotifyDamage(target, enemies, hpBefore - target.Health, hpBefore);

            if (!target.IsAlive)
                NotifyDeath(target, enemies);
        }
    }

    // Оруженосец (Light Infantry) — надевает бафф на соседнего тяжёлого пехотинца.
    // Если рядом стоят два лёгких — оба могут наделить тяжёлого баффами.
    private void ProcessSquire(LightInfantry squire, Army allies, int squireIndex)
    {
        // Шанс не сработал
        if (Rng.Next(100) >= squire.BuffChance) return;

        // Ищем тяжёлого в радиусе Range (по умолчанию 1 — соседи)
        for (int delta = -squire.Range; delta <= squire.Range; delta++)
        {
            if (delta == 0) continue;
            int j = squireIndex + delta;
            if (j < 0 || j >= allies.Units.Count) continue;

            var neighbor = allies.Units[j];
            if (!neighbor.IsAlive) continue;
            if (UnitBuff.Unwrap(neighbor) is not HeavyInfantry) continue;

            // Выбираем случайный бафф, который ещё не висит
            string[] available = { HorseBuff.Tag, SpearBuff.Tag, ShieldBuff.Tag, HelmetBuff.Tag };
            var slots = available.Where(b => !UnitBuff.HasBuff(neighbor, b)).ToList();
            if (slots.Count == 0) continue;

            string chosen = slots[Rng.Next(slots.Count)];
            Unit decorated = chosen switch
            {
                HorseBuff.Tag  => new HorseBuff(neighbor),
                SpearBuff.Tag  => new SpearBuff(neighbor),
                ShieldBuff.Tag => new ShieldBuff(neighbor),
                _              => new HelmetBuff(neighbor),
            };

            allies.Units[j] = decorated;
            Logger.BuffApplied(allies.Units[squireIndex], decorated, chosen);
            return;  // один бафф за ход
        }
    }

    private void ProcessHealer(Healer healer, Army allies, int healerIndex)
    {
        int position = healerIndex + 1;

        var candidates = new List<Unit>();
        for (int j = 0; j < allies.Units.Count; j++)
        {
            if (j == healerIndex) continue;
            var ally = allies.Units[j];
            if (!ally.IsAlive) continue;
            if (UnitBuff.Unwrap(ally) is not ICanBeHealed) continue;
            if (ally.Health >= ally.MaxHealth) continue;
            if (Math.Abs(j - healerIndex) > healer.Range) continue;

            candidates.Add(ally);
        }
        if (candidates.Count == 0) return;

        var target = candidates[Rng.Next(candidates.Count)];
        int healed = healer.UseAbility(target);
        if (healed > 0)
            Logger.HealUsed(healer, target, healed, position, healer.Range);
    }

    private void ProcessWizard(Wizard wizard, Army allies, int wizardIndex)
    {
        int position = wizardIndex + 1;

        // Защита: CloneChance в [0..100]
        int chance = Math.Max(0, Math.Min(100, wizard.CloneChance));
        if (Rng.Next(100) >= chance)
        {
            Logger.CloneFailed(wizard, position);
            return;
        }

        var candidates = new List<Unit>();
        for (int j = 0; j < allies.Units.Count; j++)
        {
            if (j == wizardIndex) continue;
            var a = allies.Units[j];
            if (!a.IsAlive) continue;
            if (UnitBuff.Unwrap(a) is not ICanBeCloned) continue;
            if (Math.Abs(j - wizardIndex) > wizard.Range) continue;
            candidates.Add(a);
        }
        if (candidates.Count == 0)
        {
            Logger.CloneFailed(wizard, position);
            return;
        }

        var original = candidates[Rng.Next(candidates.Count)];
        var inner = UnitBuff.Unwrap(original);
        var clone = ((ICanBeCloned)inner).Clone();
        allies.Units.Insert(wizardIndex, clone);
        Logger.CloneUsed(wizard, original, clone, position);
    }

    // --- Фаза 3: очистка поля + сбитие баффов от больших ударов ---
    private void CleanupPhase()
    {
        // Перед удалением — попытка сбить бафф у получивших большой урон в этом ходе
        // (упрощённое правило: если HP < 50% MaxHP — 30% шанс сбить случайный бафф)
        StripBuffsOnHardHits(Army1);
        StripBuffsOnHardHits(Army2);

        int dead1 = Army1.Units.Count(u => !u.IsAlive);
        int dead2 = Army2.Units.Count(u => !u.IsAlive);

        Army1.RemoveDeadUnits();
        Army2.RemoveDeadUnits();

        if (dead1 + dead2 > 0)
        {
            Logger.PhaseHeader("Очистка поля");
            Logger.CleanupResult(dead1, dead2, Army1, Army2);
        }
    }

    private void StripBuffsOnHardHits(Army army)
    {
        for (int i = 0; i < army.Units.Count; i++)
        {
            var u = army.Units[i];
            if (u is not UnitBuff topBuff) continue;
            if (u.MaxHealth <= 0) continue;
            // Сильный удар = HP опустилось ниже 50% от MaxHP
            if (u.Health * 2 >= u.MaxHealth) continue;
            if (Rng.Next(100) >= 30) continue;

            string strippedName = topBuff.BuffName;
            army.Units[i] = topBuff.Inner;
            Logger.BuffStripped(army.Units[i], strippedName);
        }
    }

    private Army? GetWinner()
    {
        if (Army1.IsDefeated && Army2.IsDefeated) return null;
        if (Army2.IsDefeated) return Army1;
        if (Army1.IsDefeated) return Army2;
        return null;
    }

    // --- Snapshot / Restore (для Command/Undo) ---
    private static readonly JsonSerializerOptions SnapOpts = new() { WriteIndented = false };

    public string Snapshot() => JsonSerializer.Serialize(this, SnapOpts);

    public void Restore(string snapshot)
    {
        var restored = JsonSerializer.Deserialize<Battlefield>(snapshot, SnapOpts);
        if (restored == null) return;
        Army1 = restored.Army1;
        Army2 = restored.Army2;
        TurnNumber = restored.TurnNumber;
        StrategyName = restored.StrategyName;
        // Strategy-объект восстанавливается по имени снаружи (CommandManager или Program)
    }
}
