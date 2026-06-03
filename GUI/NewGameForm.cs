using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BattleGame.Interfaces;
using BattleGame.Services;
using BattleGame.Services.Strategies;

namespace BattleGame.GUI;

/// Диалог «Новая игра»: бюджет, армии, режим, начальное построение
public sealed class NewGameForm : Form
{
    private readonly string _savesDir;
    public Battlefield? Result { get; private set; }

    // Элементы управления
    private NumericUpDown _nudBudget  = null!;
    private TextBox       _tbArmy1   = null!;
    private TextBox       _tbArmy2   = null!;
    private RadioButton   _rbRand1   = null!;
    private RadioButton   _rbMan1    = null!;
    private RadioButton   _rbRand2   = null!;
    private RadioButton   _rbMan2    = null!;
    private Button[]      _stratBtns = null!;
    private IBattleStrategy _selectedStrategy;

    // Палитра
    static readonly Color BgC    = Color.FromArgb(22, 22, 30);
    static readonly Color SurfC  = Color.FromArgb(32, 32, 44);
    static readonly Color ElevC  = Color.FromArgb(48, 48, 64);
    static readonly Color BorderC = Color.FromArgb(65, 65, 88);
    static readonly Color FgC    = Color.FromArgb(220, 220, 230);
    static readonly Color FgDimC = Color.FromArgb(120, 120, 140);
    static readonly Color GoldC  = Color.FromArgb(255, 205, 60);
    static readonly Color GreenC = Color.FromArgb(72, 200, 100);
    static readonly Color A1C    = Color.FromArgb(60, 140, 240);
    static readonly Color A2C    = Color.FromArgb(230, 70, 70);

    public NewGameForm(string savesDir)
    {
        _savesDir         = savesDir;
        _selectedStrategy = new BridgeStrategy();
        BuildLayout();
    }

    private void BuildLayout()
    {
        Text            = "Новая игра";
        Size            = new Size(580, 590);
        MinimumSize     = new Size(540, 540);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = BgC;
        ForeColor       = FgC;
        Font            = new Font("Segoe UI", 9.5f);

        // Заголовок
        var titlePanel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = Color.FromArgb(28, 28, 40)
        };
        var titleLbl = new Label
        {
            Text      = "⚔  Новая битва",
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = GoldC,
            AutoSize  = true,
            Location  = new Point(16, 12)
        };
        titlePanel.Controls.Add(titleLbl);
        titlePanel.Paint += (_, e) =>
        {
            using var pen = new Pen(BorderC, 1f);
            e.Graphics.DrawLine(pen, 0, titlePanel.Height - 1, titlePanel.Width, titlePanel.Height - 1);
        };

        // Основная область
        var content = new Panel
        {
            Location  = new Point(0, 52),
            Width     = 560,
            Height    = 470,
            BackColor = BgC
        };

        int y = 16;

        // Бюджет
        content.Controls.Add(MakeSectionLabel("Бюджет армии", 16, y));
        y += 22;

        var budgetRow = new Panel
        {
            Location  = new Point(16, y),
            Size      = new Size(520, 36),
            BackColor = SurfC
        };
        budgetRow.Paint += PaintRoundBorder;

        var lblBudLbl = new Label
        {
            Text      = "Очки на каждую армию:",
            ForeColor = FgDimC,
            AutoSize  = true,
            Location  = new Point(10, 9)
        };

        _nudBudget = new NumericUpDown
        {
            Minimum     = 50,
            Maximum     = 5000,
            Value       = 500,
            Increment   = 50,
            Location    = new Point(210, 6),
            Width       = 110,
            BackColor   = ElevC,
            ForeColor   = FgC,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 10f, FontStyle.Bold)
        };

        // Пресеты бюджета
        var budgetPresets = new (string label, int val)[] { ("300", 300), ("500", 500), ("800", 800), ("1500", 1500) };
        int bx = 340;
        foreach (var (label, val) in budgetPresets)
        {
            var pb = new Button
            {
                Text      = label,
                Location  = new Point(bx, 5),
                Size      = new Size(42, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = ElevC,
                ForeColor = FgDimC,
                Font      = new Font("Segoe UI", 8f),
                Cursor    = Cursors.Hand
            };
            pb.FlatAppearance.BorderColor = BorderC;
            var capVal = val;
            pb.Click += (_, _) => _nudBudget.Value = capVal;
            budgetRow.Controls.Add(pb);
            bx += 46;
        }

        budgetRow.Controls.Add(lblBudLbl);
        budgetRow.Controls.Add(_nudBudget);
        content.Controls.Add(budgetRow);
        y += 48;

        // Стратегия 
        content.Controls.Add(MakeSectionLabel("Начальное построение", 16, y));
        y += 22;

        var strategies = StrategyResolver.All();
        _stratBtns = new Button[strategies.Length];
        var stratRow = new Panel
        {
            Location  = new Point(16, y),
            Size      = new Size(520, 72),
            BackColor = SurfC
        };
        stratRow.Paint += PaintRoundBorder;

        int sx = 8;
        for (int i = 0; i < strategies.Length; i++)
        {
            var strat = strategies[i];
            int btnW = (520 - 16) / strategies.Length - 6;
            bool isFirst = i == 0;
            var sb = new Button
            {
                Text      = $"{strat.Name}\r\n{strat.Description}",
                Location  = new Point(sx, 8),
                Size      = new Size(btnW, 54),
                FlatStyle = FlatStyle.Flat,
                BackColor = isFirst ? Color.FromArgb(30, 60, 30) : ElevC,
                ForeColor = isFirst ? GreenC : FgDimC,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor    = Cursors.Hand,
                Tag       = strat
            };
            sb.FlatAppearance.BorderColor = isFirst ? GreenC : BorderC;
            sb.FlatAppearance.BorderSize  = isFirst ? 2 : 1;
            var capStrat = strat;
            var capI     = i;
            sb.Click += (_, _) =>
            {
                _selectedStrategy = capStrat;
                for (int j = 0; j < _stratBtns.Length; j++)
                {
                    bool sel = j == capI;
                    _stratBtns[j].BackColor             = sel ? Color.FromArgb(30, 60, 30) : ElevC;
                    _stratBtns[j].ForeColor             = sel ? GreenC : FgDimC;
                    _stratBtns[j].FlatAppearance.BorderColor = sel ? GreenC : BorderC;
                    _stratBtns[j].FlatAppearance.BorderSize  = sel ? 2 : 1;
                }
            };
            _stratBtns[i] = sb;
            stratRow.Controls.Add(sb);
            sx += btnW + 6;
        }
        content.Controls.Add(stratRow);
        y += 84;

        //  Армия 1 
        content.Controls.Add(MakeSectionLabel("Армия 1", 16, y, A1C));
        y += 22;
        var gb1 = MakeArmyBox(A1C, out _tbArmy1, out _rbRand1, out _rbMan1, "Армия 1");
        gb1.Location = new Point(16, y);
        content.Controls.Add(gb1);
        y += gb1.Height + 12;

        // Армия 2 
        content.Controls.Add(MakeSectionLabel("Армия 2", 16, y, A2C));
        y += 22;
        var gb2 = MakeArmyBox(A2C, out _tbArmy2, out _rbRand2, out _rbMan2, "Армия 2");
        gb2.Location = new Point(16, y);
        content.Controls.Add(gb2);
        y += gb2.Height + 16;

        // ── Кнопки
        var btnStart = new Button
        {
            Text      = "⚔  Начать битву!",
            Location  = new Point(16, y),
            Size      = new Size(200, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 70, 30),
            ForeColor = GreenC,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnStart.FlatAppearance.BorderColor = GreenC;
        btnStart.FlatAppearance.BorderSize  = 2;
        btnStart.Click += OnStart;

        var btnCancel = new Button
        {
            Text      = "Отмена",
            Location  = new Point(226, y),
            Size      = new Size(100, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = ElevC,
            ForeColor = FgDimC,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = BorderC;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        content.Controls.Add(btnStart);
        content.Controls.Add(btnCancel);

        Controls.Add(titlePanel);
        Controls.Add(content);
    }

    private Panel MakeArmyBox(Color accent, out TextBox nameBox, out RadioButton rbRand, out RadioButton rbMan, string defaultName)
    {
        var panel = new Panel
        {
            Size      = new Size(520, 76),
            BackColor = SurfC
        };
        panel.Paint += PaintRoundBorder;

        var lbl = new Label
        {
            Text      = "Имя:",
            ForeColor = FgDimC,
            AutoSize  = true,
            Location  = new Point(10, 12)
        };

        nameBox = new TextBox
        {
            Text        = defaultName,
            Location    = new Point(50, 9),
            Width       = 200,
            BackColor   = ElevC,
            ForeColor   = FgC,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 10f)
        };

        rbRand = new RadioButton
        {
            Text      = "Случайная",
            Checked   = true,
            Location  = new Point(270, 11),
            AutoSize  = true,
            ForeColor = FgC
        };

        rbMan = new RadioButton
        {
            Text      = "Вручную",
            Location  = new Point(376, 11),
            AutoSize  = true,
            ForeColor = FgC
        };

        // Строка описания режима
        var infoLbl = new Label
        {
            Text      = "Случайная армия: юниты подбираются автоматически по бюджету",
            ForeColor = FgDimC,
            AutoSize  = true,
            Location  = new Point(10, 42),
            Font      = new Font("Segoe UI", 8f)
        };

        // Захватываем out-параметры в локальные переменные для замыканий
        var rbManRef  = rbMan;
        var rbRandRef = rbRand;
        void UpdateInfo()
        {
            infoLbl.Text = rbManRef.Checked
                ? "Ручной режим: выберите состав армии в следующем окне"
                : "Случайная армия: юниты подбираются автоматически по бюджету";
            infoLbl.ForeColor = rbManRef.Checked ? Color.FromArgb(140, 200, 140) : FgDimC;
        }
        rbRandRef.CheckedChanged += (_, _) => UpdateInfo();
        rbManRef.CheckedChanged  += (_, _) => UpdateInfo();

        // Акцентная линия сверху
        panel.Paint += (s, e) =>
        {
            using var pen = new Pen(accent, 2f);
            e.Graphics.DrawLine(pen, 6, 0, ((Control)s!).Width - 6, 0);
        };

        panel.Controls.AddRange(new Control[] { lbl, nameBox, rbRand, rbMan, infoLbl });
        return panel;
    }

    private void OnStart(object? s, EventArgs e)
    {
        int    budget = (int)_nudBudget.Value;
        string n1     = _tbArmy1.Text.Trim().Length > 0 ? _tbArmy1.Text.Trim() : "Армия 1";
        string n2     = _tbArmy2.Text.Trim().Length > 0 ? _tbArmy2.Text.Trim() : "Армия 2";

        Army army1;
        if (_rbMan1.Checked)
        {
            using var dlg = new ManualArmyForm(n1, "A1", budget);
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return;
            army1 = dlg.Result;
        }
        else
        {
            army1 = ArmyFactory.Instance.CreateByBudget(n1, budget, "A1");
        }

        Army army2;
        if (_rbMan2.Checked)
        {
            using var dlg = new ManualArmyForm(n2, "A2", budget);
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return;
            army2 = dlg.Result;
        }
        else
        {
            army2 = ArmyFactory.Instance.CreateByBudget(n2, budget, "A2");
        }

        var bf = new Battlefield { Army1 = army1, Army2 = army2 };
        bf.SetStrategy(_selectedStrategy);

        Result       = bf;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label MakeSectionLabel(string text, int x, int y, Color? c = null) => new()
    {
        Text      = text.ToUpper(),
        Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
        ForeColor = c ?? Color.FromArgb(130, 130, 155),
        AutoSize  = true,
        Location  = new Point(x, y),
        BackColor = Color.Transparent
    };

    private static void PaintRoundBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control ctrl) return;
        using var pen = new Pen(Color.FromArgb(65, 65, 88), 1f);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
    }
}
