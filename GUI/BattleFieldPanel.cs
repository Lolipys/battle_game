using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using BattleGame.Units;

namespace BattleGame.GUI;

/// Кастомная панель: рисует поле боя двух армий с анимациями атак, урона и гибели.
/// Обновляется таймером ~25 fps.
public sealed class BattleFieldPanel : Panel
{
    // Размеры карточек 
    private const int CardW   = 132;
    private const int CardH   = 106;
    private const int CardGap = 6;
    private const int ArmyPad = 12;

    // Состояние
    private Army?   _army1;
    private Army?   _army2;
    private string? _stratName;
    private Army?   _winner;       // null = игра не окончена; ссылка = победитель; _isDraw = true = ничья
    private bool    _gameOver;
    private bool    _isDraw;

    //  Анимации 
    private readonly Dictionary<string, UnitAnim> _anims = new();
    private readonly System.Windows.Forms.Timer _ticker = new() { Interval = 100 }; // 10 fps — не перегружает UI

    private class UnitAnim
    {
        public float Damage;   // 1→0: красная вспышка
        public float Heal;     // 1→0: зелёная вспышка
        public bool  Dead;     // юнит мёртв (показываем ✖)
    }

    // Палитра
    static readonly Color BgMain   = Color.FromArgb(15, 15, 22);
    static readonly Color A1C      = Color.FromArgb(60, 130, 200);
    static readonly Color A2C      = Color.FromArgb(210, 70, 70);
    static readonly Color Gold     = Color.FromArgb(200, 165, 40);
    static readonly Color FgC      = Color.FromArgb(210, 210, 215);
    static readonly Color FgDimC   = Color.FromArgb(130, 130, 140);

    // Цвета по типу юнита
    static readonly Dictionary<string, Color> TypeColors = new()
    {
        ["ТЖ"] = Color.FromArgb(60,  110, 180),
        ["ЛГ"] = Color.FromArgb(45,  90,  170),
        ["ЛУ"] = Color.FromArgb(40,  150, 80),
        ["ХЛ"] = Color.FromArgb(30,  175, 100),
        ["МГ"] = Color.FromArgb(130, 45,  190),
        ["ГГ"] = Color.FromArgb(145, 80,  35),
    };

    // Иконки по типу юнита 
    static readonly Dictionary<string, string> TypeIcons = new()
    {
        ["ТЖ"] = "⚔",
        ["ЛГ"] = "⚡",
        ["ЛУ"] = "➶",
        ["ХЛ"] = "✚",
        ["МГ"] = "✦",
        ["ГГ"] = "⊕",
    };

    public BattleFieldPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint            |
                 ControlStyles.DoubleBuffer, true);
        BackColor = BgMain;

        _ticker.Tick += (_, _) =>
        {
            bool dirty = false;
            foreach (var a in _anims.Values)
            {
                if (a.Damage > 0) { a.Damage = MathF.Max(0, a.Damage - 0.055f); dirty = true; }
                if (a.Heal   > 0) { a.Heal   = MathF.Max(0, a.Heal   - 0.04f);  dirty = true; }
            }
            if (dirty) Invalidate();
        };
        _ticker.Start();
    }

    //  Публичный API 

    public void SetBattlefield(Army? a1, Army? a2, string? stratName = null)
    {
        _army1 = a1;
        _army2 = a2;
        if (stratName != null) _stratName = stratName;
        Invalidate();
    }

    public void SetGameOver(Army? winner, bool isDraw)
    {
        _gameOver = true;
        _winner   = winner;
        _isDraw   = isDraw;
        Invalidate();
    }

    public void ClearGameOver()
    {
        _gameOver = false;
        _winner   = null;
        _isDraw   = false;
        Invalidate();
    }

    public void AnimateDamage(string unitName)
    {
        GetAnim(unitName).Damage = 1f;
        Invalidate();
    }

    public void AnimateHeal(string unitName)
    {
        GetAnim(unitName).Heal = 1f;
        Invalidate();
    }

    public void AnimateDeath(string unitName)
    {
        GetAnim(unitName).Dead = true;
        Invalidate();
    }

    public void ClearAnimations()
    {
        _anims.Clear();
        Invalidate();
    }

    // Отрисовка 

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode       = SmoothingMode.AntiAlias;
        g.TextRenderingHint   = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode   = InterpolationMode.HighQualityBicubic;

        g.Clear(BgMain);

        if (_army1 == null && _army2 == null) { DrawEmpty(g); return; }

        int mid = Width / 2;

        // Лёгкая подсветка зон армий
        using (var b = new SolidBrush(Color.FromArgb(14, A1C)))
            g.FillRectangle(b, 0, 0, mid - 2, Height);
        using (var b = new SolidBrush(Color.FromArgb(14, A2C)))
            g.FillRectangle(b, mid + 2, 0, Width - mid - 2, Height);

        if (_army1 != null) DrawArmy(g, new Rectangle(0,       0, mid - 2, Height), _army1, A1C);
        if (_army2 != null) DrawArmy(g, new Rectangle(mid + 2, 0, Width - mid - 2, Height), _army2, A2C);

        DrawDivider(g, mid);

        if (_gameOver) DrawGameOverBanner(g);
    }

    private void DrawGameOverBanner(Graphics g)
    {
        // Тёмное наложение
        using (var ov = new SolidBrush(Color.FromArgb(160, 10, 10, 18)))
            g.FillRectangle(ov, 0, Height / 2 - 54, Width, 108);

        if (_isDraw)
        {
            DrawBannerText(g, "⚔  НИЧЬЯ  ⚔", 28f, Color.FromArgb(220, 200, 60),
                           Height / 2 - 30);
            DrawBannerText(g, "Оба войска истощены — 10 ходов без изменений", 10f,
                           Color.FromArgb(150, 150, 120), Height / 2 + 10);
        }
        else if (_winner != null)
        {
            Color wc = _winner == _army1 ? A1C : A2C;
            DrawBannerText(g, "⚑  ПОБЕДИТЕЛЬ", 13f, Color.FromArgb(180, wc), Height / 2 - 44);
            DrawBannerText(g, _winner.Name,     32f, wc,                       Height / 2 - 24);
            DrawBannerText(g, $"Выживших: {_winner.Units.Count}", 10f,
                           Color.FromArgb(160, wc), Height / 2 + 20);
        }
        else
        {
            DrawBannerText(g, "ИГРА ОКОНЧЕНА", 22f, Color.FromArgb(180, 180, 180), Height / 2 - 18);
        }
    }

    private void DrawBannerText(Graphics g, string text, float size, Color c, float y)
    {
        using var f  = new Font("Segoe UI", size, FontStyle.Bold);
        using var b  = new SolidBrush(c);
        var sz = g.MeasureString(text, f);
        g.DrawString(text, f, b, Width / 2f - sz.Width / 2, y);
    }

    private void DrawArmy(Graphics g, Rectangle zone, Army army, Color accent)
    {
        // Заголовок армии
        using var hFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        using var hBrush = new SolidBrush(accent);
        string hdr = $"  {army.Name}  ·  {army.Units.Count} юн.";
        g.DrawString(hdr, hFont, hBrush, zone.X + 4, zone.Y + 4);

        // Подчёркивание
        using var hPen = new Pen(Color.FromArgb(60, accent), 1);
        g.DrawLine(hPen, zone.X + 4, zone.Y + 24, zone.Right - 4, zone.Y + 24);

        // Сетка карточек
        int cols = Math.Max(1, (zone.Width - ArmyPad * 2) / (CardW + CardGap));
        for (int i = 0; i < army.Units.Count; i++)
        {
            int cx = zone.X + ArmyPad + (i % cols) * (CardW + CardGap);
            int cy = zone.Y + 30      + (i / cols) * (CardH + CardGap);
            if (cy + CardH > zone.Bottom - 2) break;
            DrawCard(g, army.Units[i], cx, cy, accent, i == 0);
        }
    }

    private void DrawCard(Graphics g, Unit unit, int x, int y, Color armyAccent, bool isFront)
    {
        var anim = GetAnim(unit.Name);
        string abbr = GetAbbr(unit);

        // Тень 
        using (var sb = new SolidBrush(Color.FromArgb(55, 0, 0, 0)))
            g.FillRectangle(sb, x + 3, y + 3, CardW, CardH);

        // Фон карточки 
        Color bg = Color.FromArgb(34, 34, 44);
        if (anim.Dead)
            bg = Color.FromArgb(22, 22, 28);
        else if (anim.Damage > 0.01f)
            bg = Blend(bg, Color.FromArgb(160, 30, 30), anim.Damage * 0.65f);
        else if (anim.Heal > 0.01f)
            bg = Blend(bg, Color.FromArgb(30, 130, 60), anim.Heal * 0.55f);

        using (var sb = new SolidBrush(bg))
            g.FillRectangle(sb, x, y, CardW, CardH);

        //  Подсветка передовой
        if (isFront && !anim.Dead)
        {
            using (var outer = new Pen(Color.FromArgb(50, armyAccent), 7))
                g.DrawRectangle(outer, x - 2, y - 2, CardW + 4, CardH + 4);
            using (var inner = new Pen(Color.FromArgb(190, armyAccent), 2))
                g.DrawRectangle(inner, x, y, CardW, CardH);
        }
        else
        {
            using (var pen = new Pen(Color.FromArgb(52, 52, 62)))
                g.DrawRectangle(pen, x, y, CardW, CardH);
        }

        // Заголовок типа юнита 
        Color typeC = TypeColors.GetValueOrDefault(abbr, Color.FromArgb(70, 70, 80));
        if (anim.Dead) typeC = Color.FromArgb(40, 40, 46);

        using (var sb = new SolidBrush(typeC))
            g.FillRectangle(sb, x, y, CardW, 25);

        // Иконка
        string icon = TypeIcons.GetValueOrDefault(abbr, "?");
        using (var iFont = new Font("Segoe UI Symbol", 12f))
        using (var ib    = new SolidBrush(Color.FromArgb(220, 220, 220)))
            g.DrawString(icon, iFont, ib, x + 4, y + 3);

        // Аббревиатура типа
        using (var aFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
        using (var ab    = new SolidBrush(Color.FromArgb(200, 200, 200)))
            g.DrawString(abbr, aFont, ab, x + 28, y + 8);

        //  Наложение «мёртв» 
        if (anim.Dead)
        {
            using (var ov = new SolidBrush(Color.FromArgb(160, 10, 10, 15)))
                g.FillRectangle(ov, x, y + 25, CardW, CardH - 25);
            using (var df = new Font("Segoe UI Symbol", 22f))
            using (var db = new SolidBrush(Color.FromArgb(200, 190, 50, 50)))
            {
                float tx = x + CardW / 2f - 14;
                float ty = y + 25 + (CardH - 25) / 2f - 16;
                g.DrawString("✖", df, db, tx, ty);
            }
            // Приглушённое имя
            string dname = TruncateName(UnitBuff.Unwrap(unit).Name, 16);
            using var dn = new Font("Segoe UI", 7.5f);
            using var db2 = new SolidBrush(Color.FromArgb(80, 80, 85));
            g.DrawString(dname, dn, db2, x + 4, y + 28);
            return;
        }

        // Имя юнита 
        string name = TruncateName(UnitBuff.Unwrap(unit).Name, 15);
        using (var nf = new Font("Segoe UI", 8.5f, FontStyle.Bold))
        using (var nb = new SolidBrush(FgC))
            g.DrawString(name, nf, nb, x + 4, y + 27);

        // Полоска HP 
        float pct = unit.MaxHealth > 0 ? MathF.Max(0, MathF.Min(1, (float)unit.Health / unit.MaxHealth)) : 0;
        Color hpC = pct > 0.6f ? Color.FromArgb(55, 185, 75)
                  : pct > 0.3f ? Color.FromArgb(225, 175, 25)
                                : Color.FromArgb(225, 55, 55);

        g.FillRectangle(Brushes.Black, x + 4, y + 44, CardW - 8, 8);
        using (var hpb = new SolidBrush(hpC))
            g.FillRectangle(hpb, x + 4, y + 44, (int)((CardW - 8) * pct), 8);

        // Текст HP
        using (var hpf = new Font("Consolas", 7f))
        using (var hpt = new SolidBrush(FgDimC))
            g.DrawString($"HP {unit.Health}/{unit.MaxHealth}", hpf, hpt, x + 4, y + 54);

        // Характеристики
        using (var sf = new Font("Segoe UI", 7.5f))
        using (var st = new SolidBrush(FgDimC))
            g.DrawString($"ATK {unit.Damage}   DEF {unit.Defense}", sf, st, x + 4, y + 68);

        // Баффы
        string buffs = GetBuffsShort(unit);
        if (buffs.Length > 0)
        {
            using var bf2 = new Font("Segoe UI", 7f);
            using var bt  = new SolidBrush(Color.FromArgb(145, 205, 100));
            g.DrawString(buffs, bf2, bt, x + 4, y + 83);
        }
    }

    private void DrawDivider(Graphics g, int x)
    {
        using (var pen = new Pen(Color.FromArgb(80, Gold), 2))
            g.DrawLine(pen, x, 0, x, Height);

        // Надпись VS
        using (var font = new Font("Impact", 16f, FontStyle.Bold))
        using (var brush = new SolidBrush(Color.FromArgb(110, Gold)))
        {
            var sz = g.MeasureString("VS", font);
            g.DrawString("VS", font, brush, x - sz.Width / 2, Height / 2f - sz.Height / 2);
        }

        // Имя стратегии снизу
        if (!string.IsNullOrEmpty(_stratName))
        {
            using var sf  = new Font("Segoe UI", 7.5f);
            using var sb  = new SolidBrush(Color.FromArgb(80, Gold));
            var sz = g.MeasureString(_stratName, sf);
            g.DrawString(_stratName, sf, sb, x - sz.Width / 2, Height - 20);
        }
    }

    private void DrawEmpty(Graphics g)
    {
        string txt = "Нажмите «Новая игра» для начала битвы";
        using var f = new Font("Segoe UI", 14f);
        using var b = new SolidBrush(Color.FromArgb(60, 60, 70));
        var sz = g.MeasureString(txt, f);
        g.DrawString(txt, f, b, Width / 2f - sz.Width / 2, Height / 2f - sz.Height / 2);

        // Декоративные скрещённые мечи
        using var df = new Font("Segoe UI Symbol", 42f);
        using var db = new SolidBrush(Color.FromArgb(35, 35, 45));
        var isz = g.MeasureString("⚔", df);
        g.DrawString("⚔", df, db, Width / 2f - isz.Width / 2, Height / 2f - isz.Height / 2 - 44);
    }

    // Вспомогательные методы 

    private UnitAnim GetAnim(string key)
    {
        if (!_anims.TryGetValue(key, out var a)) { a = new(); _anims[key] = a; }
        return a;
    }

    private static Color Blend(Color a, Color b, float t) =>
        Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));

    internal static string GetAbbr(Unit unit) => UnitBuff.Unwrap(unit) switch
    {
        HeavyInfantry      => "ТЖ",
        LightInfantry      => "ЛГ",
        Archer             => "ЛУ",
        Healer             => "ХЛ",
        Wizard             => "МГ",
        GulyayGorodAdapter => "ГГ",
        _                  => "??"
    };

    private static string GetBuffsShort(Unit unit)
    {
        var list = new List<string>();
        Unit c = unit;
        while (c is UnitBuff b) { list.Add(b.BuffName[..Math.Min(3, b.BuffName.Length)]); c = b.Inner; }
        return list.Count > 0 ? string.Join(" ", list.Select(s => $"+{s}")) : "";
    }

    private static string TruncateName(string s, int maxLen) =>
        s.Length <= maxLen ? s : s[..maxLen] + "…";

    protected override void Dispose(bool disposing)
    {
        if (disposing) _ticker.Dispose();
        base.Dispose(disposing);
    }
}
