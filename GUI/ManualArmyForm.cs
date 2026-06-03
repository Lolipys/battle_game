using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BattleGame.Services;
using BattleGame.Units;

namespace BattleGame.GUI;

public sealed class ManualArmyForm : Form
{
    private readonly string _armyName;
    private readonly string _tag;
    private readonly int    _totalBudget;
    private int             _remaining;
    private int             _nextNum = 1;

    private readonly RandomUnitFactory _factory = new();
    private readonly List<Unit>        _units   = new();

    public Army? Result { get; private set; }

    private Label           _lblBudget    = null!;
    private Panel           _budgetBar    = null!;
    private Panel           _budgetFill   = null!;
    private FlowLayoutPanel _unitsPanel   = null!;
    private Panel           _previewPanel = null!;
    private Button          _btnDone      = null!;
    private Button[]        _addBtns      = null!;

    private record UnitType(
        string Label, string Abbr, string Description,
        int ApproxPrice, string StatLine,
        Color Accent, Func<RandomUnitFactory, int, string, Unit> Creator);

    private readonly UnitType[] _types;

    static readonly Color BgC     = Color.FromArgb(18, 18, 26);
    static readonly Color SurfC   = Color.FromArgb(28, 28, 40);
    static readonly Color ElevC   = Color.FromArgb(44, 44, 60);
    static readonly Color BorderC = Color.FromArgb(60, 60, 80);
    static readonly Color FgC     = Color.FromArgb(220, 220, 230);
    static readonly Color FgDimC  = Color.FromArgb(110, 110, 130);
    static readonly Color GoldC   = Color.FromArgb(255, 205, 60);
    static readonly Color GreenC  = Color.FromArgb(72, 200, 100);

    public ManualArmyForm(string armyName, string tag, int budget)
    {
        _armyName    = armyName;
        _tag         = tag;
        _totalBudget = budget;
        _remaining   = budget;

        _types = new UnitType[]
        {
            new("Тяжёлая пехота", "ТЖ",
                "Танк. Высокая защита и большой запас HP.\nПолучает баффы от Лёгкой пехоты.",
                213, "HP≈152  ATK≈22  DEF≈14  Цена≈213",
                Color.FromArgb(100, 160, 220),
                (f, n, t) => f.CreateHeavyInfantry(n, t)),

            new("Лёгкая пехота", "ЛГ",
                "Оруженосец. Надевает баффы (+ATK/DEF) на соседних тяжёлых.",
                148, "HP≈90  ATK≈18  DEF≈8  Цена≈148",
                Color.FromArgb(180, 200, 140),
                (f, n, t) => f.CreateLightInfantry(n, t)),

            new("Лучник", "ЛУ",
                "Дальний бой. Стрелы игнорируют 50% защиты.\nАтакует с задних позиций.",
                81, "HP≈60  ATK≈12  DEF≈4  Цена≈81",
                Color.FromArgb(100, 200, 100),
                (f, n, t) => f.CreateArcher(n, t)),

            new("Хилер", "ХЛ",
                "Лечит союзников в радиусе.\nСлабый в прямом бою, ценен в поддержке.",
                63, "HP≈50  ATK≈7  DEF≈3  Цена≈63",
                Color.FromArgb(80, 180, 255),
                (f, n, t) => f.CreateHealer(n, t)),

            new("Маг", "МГ",
                "Клонирует союзников с шансом ~30% за ход.\nКлон появляется перед магом.",
                60, "HP≈48  ATK≈8  DEF≈3  Цена≈60",
                Color.FromArgb(180, 100, 255),
                (f, n, t) => f.CreateWizard(n, t)),

            new("Гуляй-город", "ГГ",
                "Мощный осадный юнит. Высокая защита и урон.\nДорогой, но очень эффективный.",
                245, "HP≈160  ATK≈25  DEF≈18  Цена≈245",
                Color.FromArgb(200, 160, 80),
                (f, n, t) => f.CreateGulyayGorod(n, t)),
        };

        BuildLayout();
    }

    private void BuildLayout()
    {
        Text            = $"Состав армии: {_armyName}";
        Size            = new Size(820, 680);
        MinimumSize     = new Size(740, 580);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = true;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = BgC;
        ForeColor       = FgC;
        Font            = new Font("Segoe UI", 9.5f);

        var titlePnl = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(24, 24, 36) };
        titlePnl.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderC, 1f);
            e.Graphics.DrawLine(pen, 0, titlePnl.Height - 1, titlePnl.Width, titlePnl.Height - 1);
        };
        titlePnl.Controls.Add(new Label
        {
            Text      = $"Составить армию: {_armyName}",
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = GoldC,
            AutoSize  = true,
            Location  = new Point(16, 13)
        });

        var budgetPnl = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = SurfC };
        _lblBudget = new Label
        {
            Text      = BudgetText(),
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = GoldC,
            AutoSize  = true,
            Location  = new Point(12, 8)
        };
        _budgetBar  = new Panel { Location = new Point(12, 32), Height = 8, BackColor = Color.FromArgb(30, 30, 45) };
        _budgetFill = new Panel { Location = new Point(0, 0), Height = 8, BackColor = GreenC };
        _budgetBar.Controls.Add(_budgetFill);
        budgetPnl.Controls.Add(_lblBudget);
        budgetPnl.Controls.Add(_budgetBar);
        budgetPnl.Resize += (_, _) => { _budgetBar.Width = budgetPnl.Width - 24; UpdateBudgetBar(); };

        var split = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            BackColor     = BgC,
            SplitterWidth = 4
            // Panel1MinSize / Panel2MinSize / SplitterDistance задаются в Load,
            // т.к. в конструкторе ширина ещё 0 и метод бросает исключение
        };
        Load += (_, _) =>
        {
            try
            {
                split.Panel1MinSize    = 100;
                split.Panel2MinSize    = 180;
                split.SplitterDistance = (int)(split.Width * 0.57);
            }
            catch { }
        };
        BuildLeft(split.Panel1);
        BuildRight(split.Panel2);

        Controls.Add(split);
        Controls.Add(budgetPnl);
        Controls.Add(titlePnl);

        UpdateUI();
    }

    private void BuildLeft(Panel left)
    {
        left.BackColor = BgC;
        left.Controls.Add(new Label
        {
            Text      = "ДОБАВИТЬ ЮНИТА",
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 120),
            AutoSize  = true,
            Location  = new Point(10, 10)
        });

        _addBtns = new Button[_types.Length];
        for (int i = 0; i < _types.Length; i++)
        {
            int idx  = i;
            var t    = _types[i];
            int btnW = 210;
            int btnH = 76;
            int bx   = 10 + (i % 2) * (btnW + 8);
            int by   = 34 + (i / 2) * (btnH + 6);

            var btn = new Button
            {
                Location  = new Point(bx, by),
                Size      = new Size(btnW, btnH),
                FlatStyle = FlatStyle.Flat,
                BackColor = ElevC,
                ForeColor = FgC,
                Text      = string.Empty,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left
            };
            btn.FlatAppearance.BorderColor = BorderC;
            btn.FlatAppearance.BorderSize  = 1;

            btn.Paint += (s, e) => PaintBtn(e.Graphics, (Button)s!, t);

            btn.MouseEnter += (_, _) =>
            {
                btn.FlatAppearance.BorderColor = t.Accent;
                _previewPanel.Tag = idx;
                _previewPanel.Invalidate();
            };
            btn.MouseLeave += (_, _) =>
            {
                btn.FlatAppearance.BorderColor = BorderC;
                _previewPanel.Tag = -1;
                _previewPanel.Invalidate();
            };
            btn.Click += (_, _) => AddUnit(idx);

            _addBtns[i] = btn;
            left.Controls.Add(btn);
        }
    }

    private static void PaintBtn(Graphics g, Button btn, UnitType t)
    {
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        bool enabled = btn.Enabled;
        Color ac  = enabled ? t.Accent : Color.FromArgb(55, 55, 65);
        Color fg  = enabled ? FgC      : Color.FromArgb(75, 75, 85);
        Color dim = enabled ? FgDimC   : Color.FromArgb(55, 55, 65);

        g.FillRectangle(new SolidBrush(ac), 0, 4, 3, btn.Height - 8);

        using var badgeFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        string badge  = $"[{t.Abbr}]";
        float  badgeW = g.MeasureString(badge, badgeFont).Width;
        g.FillRectangle(new SolidBrush(Color.FromArgb(enabled ? 50 : 15, ac.R, ac.G, ac.B)), 8, 8, badgeW + 4, 18);
        g.DrawString(badge, badgeFont, new SolidBrush(ac), 10f, 9f);

        using var lblFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        g.DrawString(t.Label, lblFont, new SolidBrush(fg), 8 + badgeW + 8, 10f);

        using var priceFont = new Font("Segoe UI", 8f);
        g.DrawString($"~{t.ApproxPrice} очков", priceFont, new SolidBrush(enabled ? GoldC : dim), 10f, 36f);

        using var statsFont = new Font("Segoe UI", 7.5f);
        g.DrawString(t.StatLine.Split("  ")[0], statsFont, new SolidBrush(dim), 10f, 54f);

        using var plusFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        g.DrawString("+", plusFont,
            new SolidBrush(enabled ? Color.FromArgb(100, 150, 100) : Color.FromArgb(45, 45, 55)),
            btn.Width - 30, btn.Height / 2 - 14);
    }

    private void BuildRight(Panel right)
    {
        right.BackColor = BgC;

        _previewPanel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 174,
            BackColor = SurfC,
            Tag       = -1
        };
        _previewPanel.Paint += DrawPreview;

        var listHdr = new Label
        {
            Text      = "СОСТАВ АРМИИ",
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 120),
            Dock      = DockStyle.Top,
            Height    = 26,
            TextAlign = ContentAlignment.BottomLeft,
            BackColor = BgC,
            Padding   = new Padding(6, 0, 0, 0)
        };

        _unitsPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            BackColor     = BgC,
            AutoScroll    = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents  = false,
            Padding       = new Padding(6, 4, 6, 4)
        };

        var botPnl = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = SurfC };
        botPnl.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderC, 1f);
            e.Graphics.DrawLine(pen, 0, 0, botPnl.Width, 0);
        };

        var btnRemove = new Button
        {
            Text      = "✕ Удалить последнего",
            Location  = new Point(6, 10),
            Size      = new Size(164, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 20, 20),
            ForeColor = Color.FromArgb(220, 100, 100),
            Cursor    = Cursors.Hand
        };
        btnRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 50, 50);
        btnRemove.Click += (_, _) => RemoveLast();

        _btnDone = new Button
        {
            Text      = "✔  Готово",
            Location  = new Point(178, 10),
            Size      = new Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(25, 55, 25),
            ForeColor = GreenC,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            Enabled   = false
        };
        _btnDone.FlatAppearance.BorderColor = GreenC;
        _btnDone.FlatAppearance.BorderSize  = 2;
        _btnDone.Click += OnDone;

        var btnCancel = new Button
        {
            Text      = "Отмена",
            Location  = new Point(306, 10),
            Size      = new Size(88, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = ElevC,
            ForeColor = FgDimC,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = BorderC;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        botPnl.Controls.AddRange(new Control[] { btnRemove, _btnDone, btnCancel });

        right.Controls.Add(botPnl);
        right.Controls.Add(_unitsPanel);
        right.Controls.Add(listHdr);
        right.Controls.Add(_previewPanel);
    }

    private void DrawPreview(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(SurfC);

        using var borderPen = new Pen(BorderC, 1f);
        g.DrawRectangle(borderPen, 0, 0, _previewPanel.Width - 1, _previewPanel.Height - 1);

        int idx = _previewPanel.Tag is int i ? i : -1;
        if (idx < 0 || idx >= _types.Length)
        {
            string hint = "Наведите курсор на юнита чтобы увидеть характеристики";
            var sz = g.MeasureString(hint, new Font("Segoe UI", 9f));
            g.DrawString(hint, new Font("Segoe UI", 9f), new SolidBrush(FgDimC),
                (_previewPanel.Width - sz.Width) / 2f,
                (_previewPanel.Height - sz.Height) / 2f);
            return;
        }

        var t  = _types[idx];
        int px = 14, py = 12;

        using var badgeFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        string badge  = $"[{t.Abbr}]";
        float  badgeW = g.MeasureString(badge, badgeFont).Width;
        g.DrawString(badge, badgeFont, new SolidBrush(t.Accent), px, py);
        g.DrawString(t.Label, badgeFont, new SolidBrush(FgC), px + badgeW + 6, py);

        using var divPen = new Pen(Color.FromArgb(55, 55, 78), 1f);
        g.DrawLine(divPen, px, py + 30, _previewPanel.Width - 14, py + 30);

        py += 36;
        using var descFont = new Font("Segoe UI", 8.5f);
        foreach (var line in t.Description.Split('\n'))
        {
            g.DrawString(line, descFont, new SolidBrush(FgDimC), px, py);
            py += 17;
        }

        py += 6;
        using var statFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        int sx = px;
        foreach (var part in t.StatLine.Split("  ", StringSplitOptions.RemoveEmptyEntries))
        {
            var  sz   = g.MeasureString(part, statFont);
            var  chip = new RectangleF(sx, py, sz.Width + 10, sz.Height + 6);
            g.FillRectangle(new SolidBrush(Color.FromArgb(38, 38, 56)), chip);
            using var chipPen = new Pen(Color.FromArgb(60, t.Accent.R, t.Accent.G, t.Accent.B));
            g.DrawRectangle(chipPen, chip.X, chip.Y, chip.Width, chip.Height);
            g.DrawString(part, statFont, new SolidBrush(t.Accent), sx + 5, py + 3);
            sx += (int)sz.Width + 16;
        }
    }

    private void AddUnit(int typeIdx)
    {
        var t       = _types[typeIdx];
        var sample  = t.Creator(_factory, _nextNum, _tag);

        // Открываем редактор характеристик
        using var editor = new UnitStatEditor(sample, _remaining, t.Label, t.Accent);
        if (editor.ShowDialog(this) != DialogResult.OK || editor.Result == null) return;

        var unit = editor.Result;
        if (unit.Price > _remaining)
        {
            MessageBox.Show($"Недостаточно очков!\nЦена: {unit.Price}, осталось: {_remaining}.",
                "Бюджет исчерпан", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _units.Add(unit);
        _remaining -= unit.Price;
        _nextNum++;
        AppendCard(unit, t);
        UpdateUI();
    }

    private void RemoveLast()
    {
        if (_units.Count == 0) return;
        _remaining += _units[^1].Price;
        _units.RemoveAt(_units.Count - 1);
        _nextNum--;
        if (_unitsPanel.Controls.Count > 0)
            _unitsPanel.Controls.RemoveAt(_unitsPanel.Controls.Count - 1);
        UpdateUI();
    }

    private void AppendCard(Unit unit, UnitType t)
    {
        int num  = _units.Count;
        var card = new Panel
        {
            Width     = _unitsPanel.ClientSize.Width - 12,
            Height    = 38,
            BackColor = Color.FromArgb(32, 32, 46),
            Margin    = new Padding(0, 0, 0, 3)
        };
        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(t.Accent), 0, 0, 3, card.Height);
            g.DrawString($"{num,2}. [{t.Abbr}] {unit.Name}",
                new Font("Segoe UI", 8.5f, FontStyle.Bold), new SolidBrush(FgC), 10, 4);
            g.DrawString($"HP:{unit.Health}  ATK:{unit.Damage}  DEF:{unit.Defense}  Цена:{unit.Price}",
                new Font("Segoe UI", 7.5f), new SolidBrush(FgDimC), 10, 21);
        };
        _unitsPanel.Controls.Add(card);
    }

    private void OnDone(object? s, EventArgs e)
    {
        if (_units.Count == 0) return;
        var army = new Army { Name = _armyName };
        army.Units.AddRange(_units);
        Result       = army;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateUI()
    {
        _lblBudget.Text  = BudgetText();
        UpdateBudgetBar();
        _btnDone.Enabled = _units.Count > 0;
        for (int i = 0; i < _addBtns.Length; i++)
        {
            bool can = _types[i].ApproxPrice <= _remaining;
            _addBtns[i].Enabled   = can;
            _addBtns[i].BackColor = can ? ElevC : Color.FromArgb(28, 28, 36);
            _addBtns[i].Invalidate();
        }
    }

    private void UpdateBudgetBar()
    {
        if (_budgetBar.Width <= 0) return;
        float r         = _totalBudget > 0 ? Math.Clamp((float)_remaining / _totalBudget, 0f, 1f) : 0f;
        _budgetFill.Width     = Math.Max(1, (int)(_budgetBar.Width * r));
        _budgetFill.BackColor = r > 0.5f ? GreenC
                              : r > 0.25f ? Color.FromArgb(220, 180, 40)
                                          : Color.FromArgb(220, 60, 60);
    }

    private string BudgetText() =>
        $"Бюджет: {_remaining} / {_totalBudget}  ·  потрачено: {_totalBudget - _remaining}  ·  юнитов: {_units.Count}";
}
