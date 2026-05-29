using System.Drawing;
using System.Windows.Forms;
using BattleGame.Units;

namespace BattleGame.GUI;

/// <summary>
/// Диалог кастомизации характеристик юнита перед добавлением в армию.
/// Показывает случайные базовые значения из фабрики и позволяет изменить их.
/// </summary>
public sealed class UnitStatEditor : Form
{
    private readonly Unit   _template;
    private readonly int    _budget;

    public Unit? Result { get; private set; }

    // Controls
    private NumericUpDown _nudAtk   = null!;
    private NumericUpDown _nudDef   = null!;
    private NumericUpDown _nudHp    = null!;
    private NumericUpDown _nudExtra1 = null!;   // Range / —
    private NumericUpDown _nudExtra2 = null!;   // Power / CloneChance / —
    private Label         _lblPrice  = null!;
    private Button        _btnOk     = null!;

    // Palette
    static readonly Color BgC    = Color.FromArgb(24, 24, 34);
    static readonly Color SurfC  = Color.FromArgb(34, 34, 48);
    static readonly Color ElevC  = Color.FromArgb(48, 48, 66);
    static readonly Color FgC    = Color.FromArgb(220, 220, 230);
    static readonly Color FgDimC = Color.FromArgb(120, 120, 140);
    static readonly Color GoldC  = Color.FromArgb(220, 175, 45);
    static readonly Color GreenC = Color.FromArgb(60, 190, 90);

    public UnitStatEditor(Unit template, int budget, string typeName, Color accent)
    {
        _template = template;
        _budget   = budget;
        Build(typeName, accent);
    }

    private void Build(string typeName, Color accent)
    {
        Text            = $"Характеристики: {typeName}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = BgC;
        ForeColor       = FgC;
        Font            = new Font("Segoe UI", 9.5f);

        bool isGulyay   = _template is GulyayGorodAdapter;
        bool hasRange   = _template is Archer or Healer or Wizard;
        int  extraRows  = hasRange ? 2 : 0;
        int  formH      = 290 + extraRows * 46;
        Size = new Size(370, formH);

        // ── Title bar ─────────────────────────────────────────────────────
        var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(30, 30, 44) };
        top.Paint += (_, e) =>
        {
            using var p = new Pen(Color.FromArgb(55, 55, 78), 1f);
            e.Graphics.DrawLine(p, 0, top.Height - 1, top.Width, top.Height - 1);
        };
        top.Controls.Add(new Label
        {
            Text      = $"[{BattleFieldPanel.GetAbbr(_template)}]  {_template.Name}",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = accent,
            AutoSize  = true,
            Location  = new Point(12, 10)
        });

        // ── Stat fields ───────────────────────────────────────────────────
        var body = new Panel
        {
            Location  = new Point(0, 44),
            Size      = new Size(370, formH - 44),
            BackColor = BgC
        };

        int y = 14;

        // ATK
        if (!isGulyay)
        {
            AddRow(body, "Атака (ATK):", "Урон в ближнем бою", ref y,
                   out _nudAtk, 1, 999, _template.Damage);
        }
        else
        {
            _nudAtk = new NumericUpDown { Value = 0 }; // dummy
        }

        // DEF
        AddRow(body, "Защита (DEF):", "Снижает входящий урон", ref y,
               out _nudDef, 0, 999, _template.Defense);

        // HP
        AddRow(body, "HP (здоровье):", "Максимальное количество очков здоровья", ref y,
               out _nudHp, 1, 9999, _template.Health);

        // Extra fields for ranged/heal/wizard
        if (hasRange)
        {
            string extra1Label = "Дальность:";
            string extra1Hint  = "Позиция, с которой юнит может применять способность";
            int    extra1Val   = _template switch { Archer a => a.Range, Healer h => h.Range, Wizard w => w.Range, _ => 3 };

            string extra2Label = _template is Wizard ? "Шанс клона (%):" : "Сила способности:";
            string extra2Hint  = _template is Archer ? "Урон стрелы (игнорирует 50% DEF)"
                               : _template is Healer ? "HP, восстанавливаемое за ход"
                               : "Шанс клонирования союзника за ход (%)";
            int    extra2Val   = _template switch { Archer a => a.Power, Healer h => h.Power, Wizard w => w.CloneChance, _ => 20 };

            AddRow(body, extra1Label, extra1Hint, ref y, out _nudExtra1, 1, 20, extra1Val);
            AddRow(body, extra2Label, extra2Hint, ref y, out _nudExtra2, 1, 100, extra2Val);
        }
        else
        {
            _nudExtra1 = new NumericUpDown { Value = 3 };
            _nudExtra2 = new NumericUpDown { Value = 20 };
        }

        // Price preview
        var sepLine = new Panel { Location = new Point(12, y + 4), Size = new Size(346, 1), BackColor = Color.FromArgb(50, 50, 70) };
        body.Controls.Add(sepLine);
        y += 14;

        _lblPrice = new Label
        {
            AutoSize  = true,
            Location  = new Point(12, y),
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = GoldC
        };
        body.Controls.Add(_lblPrice);
        y += 28;

        // Buttons
        _btnOk = new Button
        {
            Text      = "✔  Добавить в армию",
            Location  = new Point(12, y),
            Size      = new Size(190, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(25, 60, 25),
            ForeColor = GreenC,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _btnOk.FlatAppearance.BorderColor = GreenC;
        _btnOk.FlatAppearance.BorderSize  = 2;
        _btnOk.Click += OnOk;

        var btnCancel = new Button
        {
            Text      = "Отмена",
            Location  = new Point(210, y),
            Size      = new Size(90, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = ElevC,
            ForeColor = FgDimC,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 80);
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        body.Controls.Add(_btnOk);
        body.Controls.Add(btnCancel);

        Controls.Add(top);
        Controls.Add(body);

        // Wire value-changed events for live price update
        foreach (var n in new[] { _nudAtk, _nudDef, _nudHp, _nudExtra1, _nudExtra2 })
            n.ValueChanged += (_, _) => RefreshPrice();

        RefreshPrice();
    }

    private void AddRow(Panel parent, string label, string hint,
        ref int y, out NumericUpDown nud, int min, int max, int val)
    {
        parent.Controls.Add(new Label
        {
            Text      = label,
            ForeColor = FgC,
            AutoSize  = true,
            Location  = new Point(12, y + 3)
        });

        nud = new NumericUpDown
        {
            Minimum     = min,
            Maximum     = max,
            Value       = Math.Clamp(val, min, max),
            Location    = new Point(190, y),
            Width       = 90,
            BackColor   = ElevC,
            ForeColor   = FgC,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 10f, FontStyle.Bold)
        };
        parent.Controls.Add(nud);

        parent.Controls.Add(new Label
        {
            Text      = hint,
            ForeColor = FgDimC,
            AutoSize  = true,
            Location  = new Point(12, y + 24),
            Font      = new Font("Segoe UI", 7.5f)
        });

        y += 46;
    }

    private void RefreshPrice()
    {
        int price = CalcPrice();
        int left  = _budget - price;
        _lblPrice.Text      = $"Цена: {price}  │  Остаток бюджета: {left}";
        _lblPrice.ForeColor = price <= _budget ? GoldC : Color.FromArgb(220, 80, 80);
        _btnOk.Enabled      = price <= _budget && (int)_nudHp.Value > 0;
    }

    private int CalcPrice()
    {
        int atk = (int)_nudAtk.Value;
        int def = (int)_nudDef.Value;
        int hp  = (int)_nudHp.Value;
        return atk * 2 + (int)Math.Ceiling(def * 1.5) + hp;
    }

    private void OnOk(object? s, EventArgs e)
    {
        int    atk    = (int)_nudAtk.Value;
        int    def    = (int)_nudDef.Value;
        int    hp     = (int)_nudHp.Value;
        int    extra1 = (int)_nudExtra1.Value;
        int    extra2 = (int)_nudExtra2.Value;
        string name   = _template.Name;

        Result = _template switch
        {
            HeavyInfantry      => new HeavyInfantry(name, atk, def, hp),
            LightInfantry      => new LightInfantry(name, atk, def, hp),
            Archer             => new Archer(name,  atk, def, hp, extra1, extra2),
            Healer             => new Healer(name,  atk, def, hp, extra1, extra2),
            Wizard             => new Wizard(name,  atk, def, hp, extra1, extra2),
            GulyayGorodAdapter => new GulyayGorodAdapter(name, def, hp),
            _                  => _template
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
