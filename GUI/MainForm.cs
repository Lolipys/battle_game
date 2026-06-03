using System.Drawing;
using System.Windows.Forms;
using BattleGame.Interfaces;
using BattleGame.Services.Commands;
using BattleGame.Services.Observers;
using BattleGame.Services.Strategies;
using BattleGame.Units;

namespace BattleGame.GUI;

public sealed class MainForm : Form
{
    // Состояние
    private readonly string _savesDir;
    private Battlefield?   _bf;
    private CommandManager? _cmds;
    private GuiLogger?      _guiLogger;
    private bool            _playing;
    private bool            _bannerShown;

    // Палитра 
    static readonly Color BgC     = Color.FromArgb(20, 20, 28);
    static readonly Color SurfC   = Color.FromArgb(30, 30, 40);
    static readonly Color ElevC   = Color.FromArgb(48, 48, 60);
    static readonly Color FgC     = Color.FromArgb(210, 210, 215);
    static readonly Color FgDimC  = Color.FromArgb(110, 110, 125);
    static readonly Color GoldC   = Color.FromArgb(210, 170, 45);
    static readonly Color GreenBt = Color.FromArgb(38, 78, 38);
    static readonly Color RedBt   = Color.FromArgb(100, 30, 30);

    // Элементы управления
    private BattleFieldPanel _bfPanel  = null!;
    private RichTextBox      _log      = null!;
    private Label            _lblTurn  = null!;
    private Label            _lblStrat = null!;
    private Button           _btnSave  = null!;
    private Button           _btnTurn  = null!;
    private Button           _btnPlay  = null!;
    private Button           _btnUndo  = null!;
    private Button           _btnRedo  = null!;
    private Button           _btnReset = null!;
    private Button           _btnStrat = null!;

    public MainForm(string savesDir)
    {
        _savesDir = savesDir;
        Build();
        ShowWelcome();
    }

    //Компоновка 

    private void Build()
    {
        Text          = "Арена — Битва армий";
        Size          = new Size(1320, 860);
        MinimumSize   = new Size(980, 660);
        BackColor     = BgC;
        ForeColor     = FgC;
        Font          = new Font("Segoe UI", 9f);
        StartPosition = FormStartPosition.CenterScreen;

        //  ВЕРХНЯЯ ПАНЕЛЬ
        var top = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = SurfC };

        // Левый фиксированный блок
        var lblTitle = new Label
        {
            Text      = "⚔  АРЕНА",
            Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = GoldC,
            AutoSize  = true,
            Location  = new Point(10, 14)
        };

        var btnNew  = MakeBtn("Новая игра", 115, ElevC); btnNew.Location  = new Point(114, 10);
        var btnLoad = MakeBtn("Загрузить",   98, ElevC); btnLoad.Location = new Point(237, 10);
        _btnSave    = MakeBtn("Сохранить",   98, ElevC); _btnSave.Location = new Point(343, 10);
        _btnSave.Enabled = false;

        // Метка стратегии — растягивается по оставшемуся месту
        _lblStrat = new Label
        {
            Text      = "Построение: ─",
            ForeColor = FgDimC,
            AutoSize  = false,
            Height    = 20,
            TextAlign = ContentAlignment.MiddleLeft,
            Location  = new Point(452, 16),
            Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        // Счётчик хода — прижат к правому краю
        _lblTurn = new Label
        {
            Text      = "Ход: ─",
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = GoldC,
            AutoSize  = false,
            Width     = 110,
            Height    = 22,
            TextAlign = ContentAlignment.MiddleRight,
            Anchor    = AnchorStyles.Right | AnchorStyles.Top
        };

        void RepositionTopLabels()
        {
            _lblTurn.Location  = new Point(top.Width - 118, 15);
            _lblStrat.Width    = Math.Max(10, _lblTurn.Left - _lblStrat.Left - 8);
        }
        top.Resize += (_, _) => RepositionTopLabels();

        btnNew.Click   += OnNewGame;
        btnLoad.Click  += OnLoadGame;
        _btnSave.Click += OnSaveGame;

        top.Controls.AddRange(new Control[]
            { lblTitle, btnNew, btnLoad, _btnSave, _lblStrat, _lblTurn });

        // ПОЛЕ БОЯ 
        _bfPanel = new BattleFieldPanel
        {
            Dock      = DockStyle.Top,
            Height    = 340,
            BackColor = Color.FromArgb(15, 15, 22)
        };

        // ЛОГ 
        _log = new RichTextBox
        {
            Dock        = DockStyle.Fill,
            BackColor   = Color.FromArgb(16, 16, 22),
            ForeColor   = FgC,
            ReadOnly    = true,
            BorderStyle = BorderStyle.None,
            Font        = new Font("Consolas", 9f),
            ScrollBars  = RichTextBoxScrollBars.Vertical,
            Padding     = new Padding(6, 0, 0, 0)
        };

        // НИЖНЯЯ ПАНЕЛЬ КНОПОК
        var bot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = SurfC };

        _btnTurn  = MakeBtn("⚔  Сделать ход",  152, GreenBt);
        _btnPlay  = MakeBtn("▶▶  До конца",     130, GreenBt);
        _btnUndo  = MakeBtn("↩ Отменить",       110, ElevC);
        _btnRedo  = MakeBtn("↪ Повторить",      110, ElevC);
        _btnReset = MakeBtn("⏮ В начало",       110, ElevC);
        _btnStrat = MakeBtn("⚑  Построение",    128, Color.FromArgb(35, 50, 80));

        _btnTurn.Click  += (_, _) => OnMakeTurn();
        _btnPlay.Click  += async (_, _) => await OnPlayToEnd();
        _btnUndo.Click  += (_, _) => OnUndo();
        _btnRedo.Click  += (_, _) => OnRedo();
        _btnReset.Click += (_, _) => OnReset();
        _btnStrat.Click += (_, _) => OnChangeStrategy();

        int bx = 12;
        foreach (var b in new[] { _btnTurn, _btnPlay })
            { b.Location = new Point(bx, 10); bx += b.Width + 5; }
        bx += 14;
        foreach (var b in new[] { _btnUndo, _btnRedo, _btnReset })
            { b.Location = new Point(bx, 10); bx += b.Width + 5; }
        bx += 14;
        _btnStrat.Location = new Point(bx, 10);

        bot.Controls.AddRange(new Control[]
            { _btnTurn, _btnPlay, _btnUndo, _btnRedo, _btnReset, _btnStrat });

        // СБОРКА 
        Controls.Add(top);
        Controls.Add(bot);
        Controls.Add(_bfPanel);
        Controls.Add(_log);

        SetButtons(false);
        KeyPreview = true;
        KeyDown   += OnKeyDown;
    }

    // Горячие клавиши
    private void OnKeyDown(object? s, KeyEventArgs e)
    {
        if (!_btnTurn.Enabled) return;
        if (e.KeyCode == Keys.Space) { OnMakeTurn(); e.Handled = true; }
        if (e.KeyCode == Keys.End)   { _ = OnPlayToEnd(); e.Handled = true; }
        if (e.Control && e.KeyCode == Keys.Z) { OnUndo(); e.Handled = true; }
        if (e.Control && e.KeyCode == Keys.Y) { OnRedo(); e.Handled = true; }
    }

    // Инициализация поля боя 

    private void ShowWelcome()
    {
        Log("Добро пожаловать в Арену!", Color.White);
        Log("Нажмите «Новая игра» или «Загрузить».", FgDimC);
        Log("Горячие клавиши: Пробел — ход, End — до конца, Ctrl+Z — отменить.", FgDimC);
    }

    private void AttachBattlefield(Battlefield bf)
    {
        _bf = bf;

        // Создаём логгер
        _guiLogger = new GuiLogger(_log, RefreshUI);

        // Привязываем анимационные callbacks
        _guiLogger.VisualMeleeAttack = (att, def) =>
        {
            bool isThread = _bfPanel.InvokeRequired;
            void Act() { _bfPanel.AnimateDamage(def.Name); Sounds.Attack(); }
            if (isThread) _bfPanel.Invoke(Act); else Act();
            if (isThread && _guiLogger.AnimDelay > 0)
                Thread.Sleep(_guiLogger.AnimDelay / 3);
        };

        _guiLogger.VisualRangedAttack = (shooter, target) =>
        {
            bool isThread = _bfPanel.InvokeRequired;
            void Act() { _bfPanel.AnimateDamage(target.Name); Sounds.Arrow(); }
            if (isThread) _bfPanel.Invoke(Act); else Act();
            if (isThread && _guiLogger.AnimDelay > 0)
                Thread.Sleep(_guiLogger.AnimDelay / 4);
        };

        _guiLogger.VisualDeath = (unit, army) =>
        {
            bool isThread = _bfPanel.InvokeRequired;
            void Act() { _bfPanel.AnimateDeath(unit.Name); }
            if (isThread) _bfPanel.Invoke(Act); else Act();
            Sounds.Death();
            if (isThread && _guiLogger.AnimDelay > 0)
                Thread.Sleep(_guiLogger.AnimDelay);   // пауза, чтобы игрок успел увидеть гибель юнита
        };

        _guiLogger.VisualHeal = (healer, target, amount) =>
        {
            bool isThread = _bfPanel.InvokeRequired;
            void Act() { _bfPanel.AnimateHeal(target.Name); Sounds.Heal(); }
            if (isThread) _bfPanel.Invoke(Act); else Act();
        };

        _guiLogger.VisualClone = (wiz, orig) =>
        {
            bool isThread = _bfPanel.InvokeRequired;
            void Act() { _bfPanel.AnimateHeal(wiz.Name); Sounds.Heal(); }
            if (isThread) _bfPanel.Invoke(Act); else Act();
        };

        bf.Logger = _guiLogger;
        bf.Subscribe(new DeathObserver(_guiLogger));
        bf.Subscribe(new DamageObserver());
        _cmds = new CommandManager();
        _bfPanel.ClearAnimations();
        _bfPanel.ClearGameOver();
        _bannerShown = false;
        _log.Clear();
        RefreshUI();
        _btnSave.Enabled = true;
        SetButtons(true);
    }

    // Обновление интерфейса 

    private void RefreshUI()
    {
        if (InvokeRequired) { Invoke(RefreshUI); return; }
        if (_bf == null) return;

        // Обновляем панель поля боя
        _bfPanel.SetBattlefield(_bf.Army1, _bf.Army2, _bf.Strategy?.Name);

        // Метки
        _lblTurn.Text  = $"Ход: {_bf.TurnNumber}";
        _lblStrat.Text = $"Построение: {_bf.Strategy?.Name ?? "─"}";

        bool over = _bf.IsGameOver;
        _btnTurn.Enabled  = !over && !_playing;
        _btnPlay.Enabled  = !over && !_playing;
        _btnUndo.Enabled  = !_playing && (_cmds?.CanUndo  == true);
        _btnRedo.Enabled  = !_playing && (_cmds?.CanRedo  == true);
        _btnReset.Enabled = !_playing && (_cmds?.CanUndo  == true);
        _btnStrat.Enabled = !over && !_playing;

        if (over)
        {
            bool isDraw = _bf.DamageObserver?.IsDraw == true;
            Army? winner = isDraw ? null
                         : _bf.Army1.IsDefeated && _bf.Army2.IsDefeated ? null
                         : _bf.Army2.IsDefeated ? _bf.Army1
                         : _bf.Army1.IsDefeated ? _bf.Army2
                         : null;
            _bfPanel.SetGameOver(winner, isDraw);
            if (!_bannerShown) { ShowGameOverBanner(); _bannerShown = true; }
        }
        else
        {
            _bfPanel.ClearGameOver();
            _bannerShown = false;
        }
    }

    private void ShowGameOverBanner()
    {
        Sounds.Victory();
    }

    // Обработчики кнопок 

    private void OnNewGame(object? s, EventArgs e)
    {
        using var dlg = new NewGameForm(_savesDir);
        if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return;
        AttachBattlefield(dlg.Result);
        Log("─── Новая игра начата ───", GoldC);
        Log($"Армия 1: {_bf!.Army1.Name}  ({_bf.Army1.Units.Count} юн.) | " +
            $"Армия 2: {_bf.Army2.Name}  ({_bf.Army2.Units.Count} юн.)", FgDimC);
    }

    private void OnLoadGame(object? s, EventArgs e)
    {
        try { Directory.CreateDirectory(_savesDir); } catch { }

        if (!Directory.Exists(_savesDir) || !Directory.GetFiles(_savesDir, "*.json").Any())
        {
            MessageBox.Show("Нет сохранений.", "Загрузка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dlg = new OpenFileDialog
        {
            InitialDirectory = _savesDir,
            Filter           = "Сохранения (*.json)|*.json",
            Title            = "Загрузить игру"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        string path = dlg.FileName;
        if (!Path.HasExtension(path))
            path += ".json";

        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var bf = System.Text.Json.JsonSerializer.Deserialize<Battlefield>(json, opts);
            if (bf == null)
            {
                MessageBox.Show("Файл повреждён или пуст.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bf.SetStrategy(StrategyResolver.Resolve(bf.StrategyName));
            AttachBattlefield(bf);
            Log($"─── Загружено: {Path.GetFileNameWithoutExtension(path)}  (ход {bf.TurnNumber}) ───", GoldC);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке:\n{ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSaveGame(object? s, EventArgs e)
    {
        if (_bf == null) return;
        try { Directory.CreateDirectory(_savesDir); } catch { }

        using var dlg = new SaveFileDialog
        {
            InitialDirectory = Directory.Exists(_savesDir)
                ? _savesDir
                : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Filter       = "Сохранения (*.json)|*.json",
            FileName     = $"save_turn{_bf.TurnNumber}",
            Title        = "Сохранить игру",
            AddExtension = true,
            DefaultExt   = "json"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        string path = dlg.FileName;
        if (!Path.HasExtension(path))
            path += ".json";

        _btnSave.Enabled = false;

        try
        {
            var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(_bf, opts);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            Log($"✔ Сохранено: {Path.GetFileNameWithoutExtension(path)}", Color.FromArgb(80, 200, 80));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log("✖ Ошибка сохранения.", Color.FromArgb(200, 80, 80));
        }
        finally
        {
            _btnSave.Enabled = _bf != null;
        }
    }

    private void OnMakeTurn()
    {
        if (_bf == null || _cmds == null) return;
        _guiLogger!.AnimDelay = 0; // мгновенный режим для одного хода
        _cmds.Execute(new MakeTurnCommand(_bf, _bf.DamageObserver));
        RefreshUI();
    }

    private async Task OnPlayToEnd()
    {
        if (_bf == null || _cmds == null) return;
        _playing = true;
        _guiLogger!.AnimDelay = 320; // анимация при авто-бое
        RefreshUI();
        try
        {
            await Task.Run(() => _cmds.Execute(new PlayToEndCommand(_bf, _bf.DamageObserver)));
        }
        finally
        {
            _guiLogger.AnimDelay = 0;
            _playing = false;
            RefreshUI();
        }
    }

    private void OnUndo()
    {
        if (_cmds?.Undo() != true) { Log("Нечего отменять.", FgDimC); return; }
        _bfPanel.ClearAnimations();
        RefreshUI();
        Log($"Отменено. Ход: {_bf?.TurnNumber}", FgDimC);
    }

    private void OnRedo()
    {
        if (_cmds?.Redo() != true) { Log("Нечего повторять.", FgDimC); return; }
        _bfPanel.ClearAnimations();
        RefreshUI();
        Log($"Повторено. Ход: {_bf?.TurnNumber}", FgDimC);
    }

    private void OnReset()
    {
        _cmds?.Reset();
        _bfPanel.ClearAnimations();
        RefreshUI();
        Log($"Сброшено к началу. Ход: {_bf?.TurnNumber}", FgDimC);
    }

    private void OnChangeStrategy()
    {
        if (_bf == null || _cmds == null) return;
        var all = StrategyResolver.All();
        int btnH = 52, padding = 14;

        using var dlg = new Form
        {
            Text            = "⚑  Выбор построения",
            Size            = new Size(360, all.Length * (btnH + 4) + padding * 2 + 40),
            BackColor       = Color.FromArgb(28, 28, 38),
            ForeColor       = FgC,
            Font            = new Font("Segoe UI", 9.5f),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false, MinimizeBox = false,
            StartPosition   = FormStartPosition.CenterParent
        };

        IBattleStrategy? chosen = null;
        int dy = padding;
        foreach (var strat in all)
        {
            bool current = strat.Name == _bf.Strategy?.Name;
            var btn = new Button
            {
                Text      = $"  {strat.Name}\r\n  {strat.Description}",
                Size      = new Size(dlg.ClientSize.Width - padding * 2, btnH),
                Location  = new Point(padding, dy),
                FlatStyle = FlatStyle.Flat,
                BackColor = current ? Color.FromArgb(35, 55, 85) : ElevC,
                ForeColor = current ? Color.FromArgb(120, 185, 255) : FgC,
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn.FlatAppearance.BorderColor = current ? Color.FromArgb(60, 110, 180) : Color.FromArgb(70, 70, 85);
            btn.FlatAppearance.BorderSize  = current ? 2 : 1;
            var capture = strat;
            btn.Click += (_, _) => { chosen = capture; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(btn);
            dy += btnH + 4;
        }

        if (dlg.ShowDialog(this) == DialogResult.OK && chosen != null)
        {
            _cmds.Execute(new ChangeStrategyCommand(_bf, chosen));
            RefreshUI();
        }
    }

    // Вспомогательные методы 

    private void SetButtons(bool enabled)
    {
        foreach (var b in new[] { _btnTurn, _btnPlay, _btnUndo, _btnRedo, _btnReset, _btnStrat })
            b.Enabled = enabled;
    }

    private void Log(string text, Color c)
    {
        _log.SelectionStart  = _log.TextLength;
        _log.SelectionLength = 0;
        _log.SelectionColor  = c;
        _log.AppendText(text + "\r\n");
        _log.SelectionColor  = _log.ForeColor;
        _log.ScrollToCaret();
    }

    private static Button MakeBtn(string text, int width, Color bg) => new()
    {
        Text      = text,
        Width     = width,
        Height    = 32,
        FlatStyle = FlatStyle.Flat,
        BackColor = bg,
        ForeColor = Color.FromArgb(210, 210, 215),
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderColor = Color.FromArgb(70, 70, 88), BorderSize = 1 }
    };
}

// Звуковые эффекты через System.Media (неблокирующие) 
internal static class Sounds
{
    public static void Attack()  => Try(System.Media.SystemSounds.Beep.Play);
    public static void Arrow()   => Try(System.Media.SystemSounds.Beep.Play);
    public static void Death()   => Try(System.Media.SystemSounds.Exclamation.Play);
    public static void Heal()    => Try(System.Media.SystemSounds.Asterisk.Play);
    public static void Victory() => Try(System.Media.SystemSounds.Asterisk.Play);

    private static void Try(Action play) =>
        Task.Run(() => { try { play(); } catch { } });
}
