using System.Drawing;
using System.Windows.Forms;
using BattleGame.Interfaces;
using BattleGame.Units;

namespace BattleGame.GUI;

internal sealed class GuiLogger : IBattleLogger
{
    private readonly RichTextBox _log;
    private readonly Action _refresh;

    // Visual callbacks — set by MainForm after creating logger.
    // All callbacks run on background thread (battle thread); use Invoke if needed.
    internal Action<Unit, Unit>? VisualMeleeAttack;   // attacker, defender
    internal Action<Unit, Unit>? VisualRangedAttack;  // shooter, target
    internal Action<Unit, string>? VisualDeath;       // dying unit, army name
    internal Action<Unit, Unit, int>? VisualHeal;     // healer, target, amount
    internal Action<Unit, Unit>? VisualClone;         // wizard, original

    // When > 0 — callbacks may Thread.Sleep for animation. Set 0 for fast mode.
    internal int AnimDelay = 350;

    internal GuiLogger(RichTextBox log, Action refresh)
    {
        _log = log;
        _refresh = refresh;
    }

    private void W(string text, Color c)
    {
        if (_log.IsDisposed) return;
        void Do()
        {
            _log.SelectionStart  = _log.TextLength;
            _log.SelectionLength = 0;
            _log.SelectionColor  = c;
            _log.AppendText(text + "\r\n");
            _log.SelectionColor  = _log.ForeColor;
            _log.ScrollToCaret();
        }
        if (_log.InvokeRequired) _log.Invoke(Do); else Do();
    }

    public void TurnHeader(int turn) =>
        W($"\n  ════════════ Ход {turn} ════════════", Color.White);

    public void PhaseHeader(string phase) =>
        W($"\n  ─── {phase} ───", Color.Goldenrod);

    public void Info(string msg) => W($"  {msg}", Color.DimGray);

    public void MeleeAttack(Unit att, Unit def, int dmg, int hpB)
    {
        W($"  {att.Name} ─▶ {def.Name}", Color.Cyan);
        W($"    Урон: {att.Damage} − {def.Defense} = {dmg}  │  {def.Name}: {hpB} → {def.Health} HP", Color.Yellow);
        if (AnimDelay > 0) VisualMeleeAttack?.Invoke(att, def);
    }

    public void CounterAttack(Unit def, Unit att, int dmg, int hpB)
    {
        W($"  {def.Name} ↩ {att.Name}", Color.Orchid);
        W($"    Урон: {def.Damage} − {att.Defense} = {dmg}  │  {att.Name}: {hpB} → {att.Health} HP", Color.Yellow);
        if (AnimDelay > 0) VisualMeleeAttack?.Invoke(def, att);
    }

    public void AbilityUsed(Unit u, Unit t, int dmg, int pos, int rng, int hpB)
    {
        W($"  {u.Name} [поз.{pos}/даль.{rng}] ─▶ {t.Name}", Color.LimeGreen);
        W($"    Стрела: {dmg}  │  {t.Name}: {hpB} → {t.Health} HP", Color.Yellow);
        if (AnimDelay > 0) VisualRangedAttack?.Invoke(u, t);
    }

    public void AbilityOutOfRange(Unit u, int pos, int rng) =>
        W($"  {u.Name} [поз.{pos}/даль.{rng}] ─ не достаёт", Color.DimGray);

    public void UnitDied(Unit u, string army)
    {
        W($"  ✖  {u.Name} [{army}] погиб!", Color.OrangeRed);
        VisualDeath?.Invoke(u, army);
        _refresh();
    }

    public void CleanupResult(int d1, int d2, Army a1, Army a2) =>
        W($"  Потери: {a1.Name} −{d1}, {a2.Name} −{d2}  │  Осталось: {a1.Units.Count} / {a2.Units.Count}", Color.DimGray);

    public void GameOver(Army? winner)
    {
        W("\n  ══════ ИГРА ОКОНЧЕНА ══════", Color.White);
        if (winner != null)
        {
            W($"  Победитель: {winner.Name}!", Color.LimeGreen);
            foreach (var u in winner.Units) W($"     • {u}", Color.LimeGreen);
        }
        else W("  Ничья! Обе армии уничтожены.", Color.Gold);
        _refresh();
    }

    public void Draw(string reason)
    {
        W("\n  ══════ НИЧЬЯ ══════", Color.Gold);
        W($"  {reason}", Color.Gold);
        _refresh();
    }

    public void HealUsed(Unit h, Unit t, int hp, int pos, int rng)
    {
        W($"  {h.Name} [поз.{pos}/даль.{rng}] ─▶ лечит {t.Name}", Color.DodgerBlue);
        W($"    +{hp} HP", Color.Yellow);
        if (AnimDelay > 0) VisualHeal?.Invoke(h, t, hp);
    }

    public void CloneUsed(Unit wiz, Unit orig, Unit clone, int pos)
    {
        W($"  {wiz.Name} [поз.{pos}] ─▶ клонировал {orig.Name}!", Color.MediumAquamarine);
        W($"    Клон: {clone}", Color.MediumAquamarine);
        if (AnimDelay > 0) VisualClone?.Invoke(wiz, orig);
    }

    public void CloneFailed(Unit wiz, int pos) =>
        W($"  {wiz.Name} [поз.{pos}] ─ клонирование не сработало", Color.DimGray);

    public void BuffApplied(Unit sq, Unit t, string buff) =>
        W($"  {sq.Name} ─▶ [{buff}] → {t.Name}", Color.LawnGreen);

    public void BuffStripped(Unit t, string buff) =>
        W($"    ─ сбит [{buff}] с {t.Name}", Color.DimGray);

    public void StrategyChanged(string name) =>
        W($"\n  ⚑ Построение изменено: {name}", Color.Silver);

    public void PrintArmyStatus(Army a1, Army a2) => _refresh();
}
