using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;

namespace ProgrammingLabs.Labs.Sortix
{
     
    public abstract class SortingAlgorithm
    {
        public string Name { get; protected set; } = string.Empty;
        public bool Ascending { get; set; } = true;
        public long Iterations { get; protected set; }

        public event Action<int, int>? OnCompare;
        public event Action<int, int>? OnSwap;

        protected void RaiseCompare(int i, int j) => OnCompare?.Invoke(i, j);
        protected void RaiseSwap(int i, int j) => OnSwap?.Invoke(i, j);
        protected void RaiseIteration() { Iterations++; }

        public abstract void Sort(double[] array);

        protected bool NeedSwap(double a, double b) => Ascending ? a > b : a < b;
    }

    public class BubbleSort : SortingAlgorithm
    {
        public BubbleSort() { Name = "Пузырьковая"; }
        public override void Sort(double[] a)
        {
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - 1 - i; j++)
                {
                    RaiseCompare(j, j + 1);
                    if (NeedSwap(a[j], a[j + 1]))
                    {
                        (a[j], a[j + 1]) = (a[j + 1], a[j]);
                        RaiseSwap(j, j + 1);
                        swapped = true;
                    }
                }
                 
                RaiseIteration();
                if (!swapped) break;
            }
        }
    }

    public class InsertionSort : SortingAlgorithm
    {
        public InsertionSort() { Name = "Вставками"; }
        public override void Sort(double[] a)
        {
            for (int i = 1; i < a.Length; i++)
            {
                double key = a[i];
                int j = i - 1;
                while (j >= 0)
                {
                    RaiseCompare(j, j + 1);
                    if (!NeedSwap(a[j], key)) break;
                    a[j + 1] = a[j];
                    RaiseSwap(j, j + 1);
                    j--;
                }
                a[j + 1] = key;
                RaiseIteration();
            }
        }
    }

    public class ShakerSort : SortingAlgorithm
    {
        public ShakerSort() { Name = "Шейкерная"; }
        public override void Sort(double[] a)
        {
            int left = 0, right = a.Length - 1;
            while (left < right)
            {
                bool swapped = false;
                for (int i = left; i < right; i++)
                {
                    RaiseCompare(i, i + 1);
                    if (NeedSwap(a[i], a[i + 1]))
                    {
                        (a[i], a[i + 1]) = (a[i + 1], a[i]);
                        RaiseSwap(i, i + 1);
                        swapped = true;
                    }
                }
                right--;

                for (int i = right; i > left; i--)
                {
                    RaiseCompare(i - 1, i);
                    if (NeedSwap(a[i - 1], a[i]))
                    {
                        (a[i - 1], a[i]) = (a[i], a[i - 1]);
                        RaiseSwap(i - 1, i);
                        swapped = true;
                    }
                }
                left++;

                RaiseIteration();
                if (!swapped) break;
            }
        }
    }

    public class QuickSort : SortingAlgorithm
    {
        public QuickSort() { Name = "Быстрая"; }
        public override void Sort(double[] a) => QuickSortRec(a, 0, a.Length - 1);

        private void QuickSortRec(double[] a, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(a, low, high);
                QuickSortRec(a, low, pi - 1);
                QuickSortRec(a, pi + 1, high);
            }
        }

        private int Partition(double[] a, int low, int high)
        {
            double pivot = a[high];
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                RaiseCompare(j, high);
                bool move = Ascending ? a[j] <= pivot : a[j] >= pivot;
                if (move)
                {
                    i++;
                    if (i != j)
                    {
                        (a[i], a[j]) = (a[j], a[i]);
                        RaiseSwap(i, j);
                    }
                }
            }
            if (i + 1 != high)
            {
                (a[i + 1], a[high]) = (a[high], a[i + 1]);
                RaiseSwap(i + 1, high);
            }
            RaiseIteration();
            return i + 1;
        }
    }

    public class BogoSort : SortingAlgorithm
    {
        private readonly Random _rnd = new Random();
        public BogoSort() { Name = "BOGO"; }

        public override void Sort(double[] a)
        {
            var start = DateTime.Now;
            while (true)
            {
                RaiseIteration();
                if (IsSorted(a)) break;
                if ((DateTime.Now - start).TotalSeconds > 10)
                    throw new TimeoutException("BOGO превысила лимит 10 секунд.");
                Shuffle(a);
            }
        }

        private void Shuffle(double[] a)
        {
            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (a[i], a[j]) = (a[j], a[i]);
                RaiseSwap(i, j);
            }
        }

        private bool IsSorted(double[] a)
        {
            for (int i = 0; i < a.Length - 1; i++)
            {
                RaiseCompare(i, i + 1);
                if (Ascending && a[i] > a[i + 1]) return false;
                if (!Ascending && a[i] < a[i + 1]) return false;
            }
            return true;
        }
    }

    public class AlgorithmState
    {
        public string Name = string.Empty;
        public Color BarColor;
        public double[] Data = Array.Empty<double>();
        public int CompareI = -1, CompareJ = -1;
        public int SwapI = -1, SwapJ = -1;
        public long Comparisons;
        public long Swaps;
        public long Iterations;
        public double AvgMs;
        public double MinMs;
        public double MaxMs;
        public int Runs;
        public bool Finished;
        public string? Error;
        public bool Visualize;

        public readonly object Sync = new object();

        public List<(int type, int i, int j)> EventLog = new();
        public readonly object LogSync = new object();

        public (double[] data, int ci, int cj, int si, int sj,
                long cmp, long swp, long iter,
                double avg, double min, double max, int runs,
                bool finished, string? err) Snapshot()
        {
            lock (Sync)
            {
                var copy = new double[Data.Length];
                Array.Copy(Data, copy, Data.Length);
                return (copy, CompareI, CompareJ, SwapI, SwapJ,
                        Comparisons, Swaps, Iterations,
                        AvgMs, MinMs, MaxMs, Runs,
                        Finished, Error);
            }
        }
    }

    public partial class SortixForm : Form
    {
        private static readonly Color BgDark = Color.FromArgb(49, 53, 39);
        private static readonly Color BgPanel = Color.FromArgb(61, 66, 50);
        private static readonly Color BgCard = BgPanel;
        private static readonly Color BgInput = Color.FromArgb(46, 52, 41);
        private static readonly Color Accent = Color.FromArgb(177, 190, 137);
        private static readonly Color AccentLight = Color.FromArgb(83, 96, 70);
        private static readonly Color TextMain = Color.FromArgb(220, 222, 202);
        private static readonly Color TextDim = Color.FromArgb(172, 183, 156);
        private static readonly Color GridLine = Color.FromArgb(107, 115, 91);
        private static readonly Color Canvas = Color.FromArgb(44, 48, 37);
        private static readonly Color ChartBg = Color.FromArgb(54, 59, 45);
        private static readonly Color ChartGrid = Color.FromArgb(85, 93, 74);
        private static readonly Color CanvasText = Color.FromArgb(221, 224, 205);
        private static readonly Color CanvasMuted = Color.FromArgb(168, 180, 153);
        private static readonly Color CompareClr = Color.FromArgb(173, 205, 209);
        private static readonly Color SwapClr = Color.FromArgb(220, 165, 94);

        private const int MaxBogoElements = 100;
        private const int MaxVisualizeElements = 50;
        private const int RunsPerAlgorithm = 5;

        private Panel headerPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Button btnBackMenu = null!;

        private Panel leftPanel = null!;
        private Panel toolbarPanel = null!;
        private Button btnGenerate = null!;
        private Button btnExcel = null!;
        private Button btnGoogle = null!;
        private Button btnClear = null!;
        private DataGridView dataGrid = null!;
        private readonly Font _rowDeleteFont = new Font("Consolas", 10F, FontStyle.Bold);
        private Label lblCount = null!;

        private Panel optionsPanel = null!;
        private CheckBox cbBubble = null!;
        private CheckBox cbInsertion = null!;
        private CheckBox cbShaker = null!;
        private CheckBox cbQuick = null!;
        private CheckBox cbBogo = null!;
        private Label lblDirection = null!;
        private ComboBox cmbDirection = null!;
        private Label lblDelay = null!;
        private Label lblDelayValue = null!;
        private TrackBar tbDelay = null!;

        private Label lblDecimals = null!;
        private NumericUpDown nudDecimals = null!;

        private Panel workspacePanel = null!;
        private Label workspaceHint = null!;
        private Label lblStatus = null!;

        private Button btnCalculate = null!;
        private Button btnStop = null!;

        private double[] _currentData = Array.Empty<double>();
        private List<AlgorithmState> _states = new();
        private System.Windows.Forms.Timer _renderTimer = null!;
        private bool _isFullscreen = false;
        private bool _isRunning;
        private CancellationTokenSource? _cts;
        private int _decimalPlaces = 3;
        private int _hoveredDeleteRow = -1;
 
        private readonly Dictionary<string, Color> _algoColors = new()
        {
            ["Пузырьковая"] = Color.FromArgb(183, 193, 150),
            ["Вставками"] = Color.FromArgb(203, 207, 173),
            ["Шейкерная"] = Color.FromArgb(152, 175, 122),
            ["Быстрая"] = Color.FromArgb(197, 208, 156),
            ["BOGO"] = Color.FromArgb(214, 176, 115)
        };
        private Panel stagePanel = null!;
        private Label dataTitle = null!, algoTitle = null!, stageTitle = null!;
        private Button btnReport = null!;
        private TextBox reportBox = null!;
        private readonly ToolTip chartTip = new ToolTip { InitialDelay = 120, ReshowDelay = 80, AutoPopDelay = 10000 };
        private readonly List<(AlgorithmState state, Rectangle area)> _chartAreas = new();
        private bool _showReport;
        private const string EmptyHint = "Нет данных";

        public SortixForm()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sortix";
            AutoScaleMode = AutoScaleMode.None;
            MinimumSize = new Size(U(1020), U(640));
            Size = new Size(U(1200), U(760));
            BackColor = BgDark;
            KeyPreview = true;
            HandleCreated += (s, e) => SetDarkTitleBar(this);
            if (IsHandleCreated) SetDarkTitleBar(this);

            BuildUi();
            SetupGrid();

            dataGrid.CellValueNeeded += DataGrid_CellValueNeeded;
            dataGrid.CellValuePushed += DataGrid_CellValuePushed;
            dataGrid.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Delete || dataGrid.IsCurrentCellInEditMode || _isRunning)
                    return;
                int index = dataGrid.CurrentCell?.RowIndex ?? -1;
                if (index >= 0 && index < _currentData.Length)
                {
                    DeleteNumber(index);
                    e.Handled = true;
                }
            };

            KeyDown += SortixForm_KeyDown;
            Resize += (s, e) => LayoutControls();
            DpiChanged += (s, e) => { MinimumSize = new Size(U(1020), U(640)); LayoutControls(); };
            FormClosing += (s, e) =>
            {
                if (!_isRunning) return;
                e.Cancel = true;
                _cts?.Cancel();
                lblStatus.Text = "Остановка сортировки...";
            };
            FormClosed += (s, e) => { _renderTimer.Dispose(); chartTip.Dispose(); _cts?.Dispose(); _rowDeleteFont.Dispose(); };

            _renderTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _renderTimer.Tick += (s, e) =>
            {
                if (_isRunning) workspacePanel.Invalidate();
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var area = Screen.FromControl(this).WorkingArea;
            Size = new Size(Math.Min(Width, area.Width - 24), Math.Min(Height, area.Height - 48));
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
            LayoutControls();
        }

        private void EnterFullscreen()
        {
            var screen = Screen.FromControl(this);
            FormBorderStyle = FormBorderStyle.None;
            Bounds = screen.Bounds;
            WindowState = FormWindowState.Normal;
            _isFullscreen = true;
        }

        private void ExitFullscreen()
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;

            var screen = Screen.FromControl(this);
            int w = Math.Min(1200, screen.WorkingArea.Width - 100);
            int h = Math.Min(700, screen.WorkingArea.Height - 100);
            Size = new Size(w, h);
            Location = new Point(
                screen.WorkingArea.Left + (screen.WorkingArea.Width - w) / 2,
                screen.WorkingArea.Top + (screen.WorkingArea.Height - h) / 2);

            _isFullscreen = false;
            LayoutControls();
        }

        private int U(float value) => (int)Math.Round(value * DeviceDpi / 96f);

        private sealed class Surface : Panel
        {
            public Surface() { DoubleBuffered = true; ResizeRedraw = true; }
        }

        private Label LabelAt(string text, float size, Color color, Control parent, bool bold = false)
        {
            var label = new Label { Text = text, AutoSize = true, ForeColor = color,
                BackColor = Color.Transparent, Font = new Font("Tahoma", size, bold ? FontStyle.Bold : FontStyle.Regular) };
            parent.Controls.Add(label); return label;
        }

        private Button MakeSmallButton(string text)
        {
            var button = new Button {
                Text = text, FlatStyle = FlatStyle.Flat, ForeColor = TextMain, BackColor = BgInput,
                Font = new Font("Tahoma", 9F), Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = GridLine;
            button.FlatAppearance.MouseOverBackColor = AccentLight;
            return button;
        }

        private CheckBox MakeAlgoCheck(string name, bool selected)
        {
            var check = new CheckBox {
                Text = name, Checked = selected, Appearance = Appearance.Button,
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 9F), BackColor = BgInput, ForeColor = TextMain,
                Cursor = Cursors.Hand
            };
            check.FlatAppearance.BorderColor = GridLine;
            check.FlatAppearance.CheckedBackColor = AccentLight;
            optionsPanel.Controls.Add(check);
            return check;
        }

        private void BuildUi()
        {
            SuspendLayout();
            headerPanel = new Surface { Dock = DockStyle.Top, BackColor = BgDark };
            
            titleLabel = LabelAt("Sortix  /  консоль сортировки", 17, Accent, headerPanel, true);
            subtitleLabel = LabelAt("Пять алгоритмов", 9, TextDim, headerPanel);
            btnBackMenu = MakeSmallButton("В меню");
            btnBackMenu.Click += (s, e) => Close();
            headerPanel.Controls.Add(btnBackMenu);
            leftPanel = new Surface { BackColor = BgCard };
            optionsPanel = new Surface { BackColor = BgCard };
            stagePanel = new Surface { BackColor = Canvas };
            foreach (var panel in new[] { leftPanel, optionsPanel, stagePanel })
            {
                panel.Paint += CardPanel_Paint;
            }
            dataTitle = LabelAt("Числа", 10, TextMain, leftPanel, true);
            algoTitle = LabelAt("Алгоритмы", 10F, TextMain, optionsPanel, true);
            stageTitle = LabelAt("Визуализация", 11, CanvasText, stagePanel, true);
            btnReport = MakeSmallButton("Отчёт"); btnReport.Enabled = false;
            btnReport.BackColor = Canvas; btnReport.Click += (s, e) => { _showReport = !_showReport; UpdateWorkspace(); };
            stagePanel.Controls.Add(btnReport);

            toolbarPanel = new Panel { BackColor = BgCard };
            btnGenerate = MakeSmallButton("Генерация");
            btnExcel = MakeSmallButton("Excel"); btnGoogle = MakeSmallButton("Google");
            btnClear = MakeSmallButton("Очистить"); btnClear.CausesValidation = false;
            btnGenerate.Click += BtnGenerate_Click; btnExcel.Click += BtnExcel_Click;
            btnGoogle.Click += BtnGoogle_Click; btnClear.Click += BtnClear_Click;
            toolbarPanel.Controls.AddRange(new Control[] { btnGenerate, btnClear, btnExcel, btnGoogle });
            leftPanel.Controls.Add(toolbarPanel);
            dataGrid = new DataGridView
            {
                VirtualMode = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                EditMode = DataGridViewEditMode.EditOnEnter,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = U(28), RowHeadersVisible = true, RowHeadersWidth = U(34),
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
                BackgroundColor = BgCard, GridColor = GridLine, BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.None, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, AllowUserToResizeRows = false,
                EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9.5F), ForeColor = TextMain
            };
            dataGrid.RowTemplate.Height = U(23);
            dataGrid.DefaultCellStyle.BackColor = BgCard; dataGrid.DefaultCellStyle.ForeColor = TextMain;
            dataGrid.DefaultCellStyle.SelectionBackColor = AccentLight; dataGrid.DefaultCellStyle.SelectionForeColor = TextMain;
            dataGrid.DefaultCellStyle.Padding = new Padding(U(5), 0, U(5), 0);
            dataGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(54, 59, 45);
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = BgInput;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextDim;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = BgInput;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold);
            dataGrid.RowHeadersDefaultCellStyle.BackColor = BgInput;
            dataGrid.RowHeadersDefaultCellStyle.ForeColor = TextDim;
            dataGrid.RowHeadersDefaultCellStyle.SelectionBackColor = AccentLight;
            dataGrid.RowHeadersDefaultCellStyle.SelectionForeColor = TextMain;
            dataGrid.CellPainting += (s, e) =>
            {
                if (e.ColumnIndex != -1 || e.RowIndex < 0 || e.Graphics == null) return;
                using var background = new SolidBrush(BgCard);
                e.Graphics.FillRectangle(background, e.CellBounds);

                bool isNew = e.RowIndex >= _currentData.Length;
                bool active = !isNew && (e.RowIndex == _hoveredDeleteRow ||
                    dataGrid.CurrentCell?.RowIndex == e.RowIndex);

                if (active)
                {
                    var deleteRect = new Rectangle(e.CellBounds.Left + U(2), e.CellBounds.Top,
                        U(15), e.CellBounds.Height);
                    TextRenderer.DrawText(e.Graphics, "−", _rowDeleteFont,
                        deleteRect, SwapClr,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                var numberRect = new Rectangle(e.CellBounds.Left + (isNew ? 0 : U(15)), e.CellBounds.Top,
                    Math.Max(1, e.CellBounds.Width - (isNew ? 0 : U(15))), e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, isNew ? "+" : (e.RowIndex + 1).ToString(),
                    dataGrid.Font, numberRect, TextDim,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            };
            dataGrid.CellMouseMove += (s, e) =>
            {
                int row = e.ColumnIndex == -1 && e.RowIndex >= 0 && e.RowIndex < _currentData.Length
                    ? e.RowIndex : -1;
                if (_hoveredDeleteRow != row)
                {
                    int old = _hoveredDeleteRow; _hoveredDeleteRow = row;
                    if (old >= 0 && old < dataGrid.Rows.Count) dataGrid.InvalidateCell(-1, old);
                    if (row >= 0) dataGrid.InvalidateCell(-1, row);
                }
            };
            dataGrid.MouseLeave += (s, e) =>
            {
                int old = _hoveredDeleteRow; _hoveredDeleteRow = -1;
                if (old >= 0 && old < dataGrid.Rows.Count) dataGrid.InvalidateCell(-1, old);
            };
            dataGrid.CurrentCellChanged += (s, e) =>
            {
                 
                dataGrid.Invalidate();
            };
            dataGrid.CellMouseClick += (s, e) =>
            {
                if (_isRunning || e.Button != MouseButtons.Left || e.ColumnIndex != -1 || e.RowIndex < 0) return;
                if (e.RowIndex >= _currentData.Length || e.X > U(17)) return;
                DeleteNumber(e.RowIndex);
            };
            dataGrid.CellValidating += DataGrid_CellValidating;
            SetDoubleBuffered(dataGrid); leftPanel.Controls.Add(dataGrid);
            lblCount = LabelAt("Элементов: 0", 9, TextDim, leftPanel);
            cbBubble = MakeAlgoCheck("Пузырьковая", true);
            cbInsertion = MakeAlgoCheck("Вставками", false);
            cbShaker = MakeAlgoCheck("Шейкерная", false);
            cbQuick = MakeAlgoCheck("Быстрая", true);
            cbBogo = MakeAlgoCheck("BOGO", false);

            lblDirection = LabelAt("ПОРЯДОК", 8, TextDim, optionsPanel, true);
            cmbDirection = new ComboBox {
                DropDownStyle = ComboBoxStyle.DropDownList, BackColor = BgInput, ForeColor = TextMain,
                FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9F)
            };
            cmbDirection.Items.AddRange(new[] { "По возрастанию", "По убыванию" }); cmbDirection.SelectedIndex = 0;
            lblDecimals = LabelAt("ЗНАКОВ ПОСЛЕ ЗАПЯТОЙ", 8, TextDim, optionsPanel, true);
            nudDecimals = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 3,
                BackColor = BgInput, ForeColor = TextMain, BorderStyle = BorderStyle.None,
                TextAlign = System.Windows.Forms.HorizontalAlignment.Center, Font = new Font("Consolas", 9.5F) };
            lblDelay = LabelAt("ЗАДЕРЖКА", 8, TextDim, optionsPanel, true);
            lblDelayValue = LabelAt("15 мс", 9, TextMain, optionsPanel, true);
            tbDelay = new TrackBar { Minimum = 0, Maximum = 100, Value = 15, TickStyle = TickStyle.None,
                BackColor = BgCard, AccessibleName = "Задержка" };
            tbDelay.ValueChanged += (s, e) => lblDelayValue.Text = $"{tbDelay.Value} мс";
            optionsPanel.Controls.AddRange(new Control[] { cmbDirection, nudDecimals, tbDelay });
            nudDecimals.ValueChanged += (s, e) =>
            {
                _decimalPlaces = (int)nudDecimals.Value;
                if (_currentData.Length > 0)
                {
                    for (int i = 0; i < _currentData.Length; i++)
                        _currentData[i] = Math.Round(_currentData[i], _decimalPlaces, MidpointRounding.AwayFromZero);
                    dataGrid.Invalidate();
                }
                workspacePanel.Invalidate();
            };

            workspacePanel = new Surface { BackColor = Canvas, AutoScroll = true };
            workspacePanel.Paint += WorkspacePanel_Paint;
            workspacePanel.MouseMove += ChartMouseMove;
            workspacePanel.MouseLeave += (s, e) => chartTip.SetToolTip(workspacePanel, "");
            workspaceHint = new Label { Text = EmptyHint, Font = new Font("Consolas", 10F),
                ForeColor = CanvasMuted, BackColor = Canvas, TextAlign = ContentAlignment.MiddleCenter };
            workspacePanel.Controls.Add(workspaceHint); stagePanel.Controls.Add(workspacePanel);
            reportBox = new TextBox { Multiline = true, ReadOnly = true, WordWrap = false,
                ScrollBars = ScrollBars.Both, BorderStyle = BorderStyle.None, BackColor = Canvas,
                ForeColor = CanvasText, Font = new Font("Consolas", 9.5F), Visible = false };
            stagePanel.Controls.Add(reportBox);
            btnCalculate = MakeSmallButton("Запустить");
            btnCalculate.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            btnCalculate.BackColor = Accent; btnCalculate.ForeColor = BgDark;
            btnStop = MakeSmallButton("Стоп"); btnStop.BackColor = BgDark; btnStop.Enabled = false;
            btnCalculate.Click += BtnCalculate_Click; btnStop.Click += BtnStop_Click;
            lblStatus = new Label { Text = "Готово к запуску", Font = new Font("Consolas", 9F),
                ForeColor = TextDim, BackColor = BgDark, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
            
            Controls.AddRange(new Control[] { leftPanel, stagePanel, optionsPanel, lblStatus, btnStop, btnCalculate, headerPanel });
            headerPanel.BringToFront(); ResumeLayout(false);
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(control, true, null);
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll", ExactSpelling = true)]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

        private static void SetDarkTitleBar(Form form)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;
            int dark = 1, background = ColorTranslator.ToWin32(BgDark), text = ColorTranslator.ToWin32(TextMain);
            DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int));
            DwmSetWindowAttribute(form.Handle, 35, ref background, sizeof(int));
            DwmSetWindowAttribute(form.Handle, 36, ref text, sizeof(int));
        }

        internal static void ApplyDialogStyle(Form dialog)
        {
            dialog.BackColor = BgDark; dialog.ForeColor = TextMain;
            dialog.HandleCreated += (s, e) => SetDarkTitleBar(dialog);
            if (dialog.IsHandleCreated) SetDarkTitleBar(dialog);
            void Style(Control c)
            {
                c.ForeColor = Equals(c.Tag, "muted") ? TextDim : TextMain;
                c.BackColor = c is TextBoxBase || c is NumericUpDown ? BgInput : BgDark;
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.BackColor = b.DialogResult == DialogResult.OK ? Accent : BgInput;
                    b.ForeColor = b.DialogResult == DialogResult.OK ? BgDark : TextMain;
                    b.FlatAppearance.BorderColor = GridLine;
                    b.Cursor = Cursors.Hand;
                }
                if (c is NumericUpDown) return;
                foreach (Control child in c.Controls) Style(child);
            }
            foreach (Control c in dialog.Controls) Style(c);
        }

        internal static TableLayoutPanel CreateDialogBody(Form dialog, string title, Size size, int rows)
        {
            dialog.SuspendLayout();
            dialog.AutoScaleDimensions = new SizeF(96F, 96F);
            dialog.AutoScaleMode = AutoScaleMode.Dpi;
            dialog.Font = new Font("Consolas", 9F);
            dialog.Text = title; dialog.ClientSize = size; dialog.Padding = new Padding(18);
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.MaximizeBox = false; dialog.MinimizeBox = false; dialog.ShowInTaskbar = false;
            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = rows,
                Margin = Padding.Empty, Padding = Padding.Empty };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dialog.Controls.Add(body); return body;
        }

        internal static FlowLayoutPanel CreateDialogActions(Button accept, Button cancel)
        {
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, Margin = new Padding(0, 14, 0, 0), Padding = Padding.Empty };
            foreach (var button in new[] { cancel, accept })
            {
                button.Size = new Size(112, 34); button.Margin = new Padding(8, 0, 0, 0);
                actions.Controls.Add(button);
            }
            cancel.CausesValidation = false;
            return actions;
        }

        private static string FormatTime(double ms)
        {
            if (ms < 0) ms = 0;

            if (ms < 1000.0)
                return $"{ms:F4} мс";

            double sec = ms / 1000.0;
            if (sec < 60.0)
                return $"{sec:F4} с";

            double min = Math.Floor(sec / 60.0);
            double remSec = sec - min * 60.0;
            return $"{min:F0} мин {remSec:F2} с";
        }

        private static bool TryParseDouble(string s, out double value)
        {
            s = s.Trim();
            return
                double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.CurrentCulture, out value) ||
                double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private void LayoutControls()
        {
            if (workspacePanel == null || btnCalculate == null) return;
            int m = U(12), gap = U(10), top = U(68), bottom = ClientSize.Height - m;
            headerPanel.Height = U(58);
            titleLabel.Location = new Point(U(14), U(7)); subtitleLabel.Location = new Point(U(18), U(37));
            btnBackMenu.Bounds = new Rectangle(U(345), U(11), U(100), U(30));
            int leftW = Math.Max(U(248), Math.Min(U(300), ClientSize.Width / 5));
            leftPanel.Bounds = new Rectangle(m, top, leftW, Math.Max(U(220), bottom - top));
            int x = leftPanel.Right + gap, w = Math.Max(U(680), ClientSize.Width - m - x);
            optionsPanel.Bounds = new Rectangle(x, bottom - U(52 + 10 + 142), w, U(142));
            stagePanel.Bounds = new Rectangle(x, top, w, Math.Max(U(100), optionsPanel.Top - gap - top));
            stageTitle.Location = new Point(U(12), U(9));
            btnReport.Bounds = new Rectangle(w - U(115), U(8), U(103), U(28));
            workspacePanel.Bounds = new Rectangle(U(8), U(43), w - U(16), Math.Max(U(30), stagePanel.Height - U(53)));
            reportBox.Bounds = new Rectangle(U(12), U(45), w - U(24), Math.Max(U(30), stagePanel.Height - U(55)));
            dataTitle.Location = new Point(U(14), U(12));
            toolbarPanel.Bounds = new Rectangle(U(12), U(48), leftW - U(24), U(70));
            int first = (toolbarPanel.Width - U(8)) * 3 / 5;
            btnGenerate.Bounds = new Rectangle(0, 0, first, U(31));
            btnClear.Bounds = new Rectangle(first + U(8), 0, toolbarPanel.Width - first - U(8), U(31));
            int half = (toolbarPanel.Width - U(8)) / 2;
            btnExcel.Bounds = new Rectangle(0, U(36), half, U(31));
            btnGoogle.Bounds = new Rectangle(half + U(8), U(36), toolbarPanel.Width - half - U(8), U(31));
            dataGrid.Bounds = new Rectangle(U(12), U(131), leftW - U(24), Math.Max(U(60), leftPanel.Height - U(166)));
            lblCount.Location = new Point(U(20), leftPanel.Height - U(31));

            algoTitle.Location = new Point(U(14), U(8));
            var choices = new[] { cbBubble, cbInsertion, cbShaker, cbQuick, cbBogo };
            int tileW = (w - U(36 + 8 * 4)) / 5;
            for (int i = 0; i < choices.Length; i++)
                choices[i].Bounds = new Rectangle(U(18) + i * (tileW + U(8)), U(32), tileW, U(36));
            int col1 = U(18), col2 = U(217), col3 = U(411);
            lblDirection.Location = new Point(col1, U(77));
            cmbDirection.Bounds = new Rectangle(col1, U(99), U(177), U(27));
            lblDecimals.Location = new Point(col2, U(77));
            nudDecimals.Bounds = new Rectangle(col2, U(99), U(60), U(24));
            lblDelay.Location = new Point(col3, U(77));
            lblDelayValue.Location = new Point(w - U(76), U(77));
            tbDelay.Bounds = new Rectangle(col3 - U(8), U(99), w - col3 - U(12), U(29));
            btnCalculate.Bounds = new Rectangle(ClientSize.Width - m - U(170), bottom - U(44), U(170), U(44));
            btnStop.Bounds = new Rectangle(btnCalculate.Left - U(105), btnCalculate.Top + U(4), U(93), U(36));
            lblStatus.Bounds = new Rectangle(x + U(3), btnCalculate.Top, Math.Max(U(100), btnStop.Left - x - U(20)), U(52));
            workspacePanel.Invalidate();
        }

        private void CardPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p || p.Width < 2 || p.Height < 2) return;
            using var pen = new Pen(p == stagePanel ? ChartGrid : GridLine);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        private void UpdateWorkspace()
        {
            reportBox.Visible = _showReport; workspacePanel.Visible = !_showReport;
            btnReport.Text = _showReport ? "График" : "Отчёт";
            workspaceHint.Text = _states.Count > 0 ? "Визуализация доступна до 50 элементов.\nРезультаты расчёта — в отчёте." : EmptyHint;
            workspaceHint.Visible = !_states.Any(s => s.Visualize);
            workspacePanel.AutoScrollMinSize = Size.Empty;
            workspacePanel.Invalidate();
        }

        private void WorkspacePanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Canvas);
            _chartAreas.Clear();
            var states = _states.Where(s => s.Visualize).ToList();
            if (states.Count == 0)
            {
                workspacePanel.AutoScrollMinSize = Size.Empty;
                int w = workspacePanel.ClientSize.Width, h = workspacePanel.ClientSize.Height;
                g.DrawRectangle(Pens.DarkOliveGreen, U(18), U(18),
                    Math.Max(1, w - U(36)), Math.Max(1, h - U(36)));
                workspaceHint.Bounds = new Rectangle(U(28), Math.Max(0, h / 2 - U(35)),
                    Math.Max(1, w - U(56)), Math.Min(U(72), h));
                workspaceHint.Visible = true; return;
            }
            workspaceHint.Visible = false;
            int pad = U(10), available = workspacePanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            int cols = Math.Min(states.Count, available >= U(930) ? 3 : available >= U(580) ? 2 : 1);
            int rows = (states.Count + cols - 1) / cols;
            int cellW = Math.Max(1, (available - pad * (cols + 1)) / cols);
            int cellH = Math.Max(U(194), (workspacePanel.ClientSize.Height - pad * (rows + 1)) / rows);
            var size = new Size(0, rows * (cellH + pad) + pad);
            if (workspacePanel.AutoScrollMinSize != size) workspacePanel.AutoScrollMinSize = size;
            for (int i = 0; i < states.Count; i++)
            {
                var area = new Rectangle(pad + i % cols * (cellW + pad) + workspacePanel.AutoScrollPosition.X,
                    pad + i / cols * (cellH + pad) + workspacePanel.AutoScrollPosition.Y, cellW, cellH);
                _chartAreas.Add((states[i], area));
                if (area.IntersectsWith(workspacePanel.ClientRectangle)) DrawAlgorithm(g, states[i], area);
            }
        }

        private Rectangle PlotBounds(Rectangle area) => new Rectangle(area.Left + U(13), area.Top + U(57),
            Math.Max(1, area.Width - U(26)), Math.Max(1, area.Height - U(119)));

        private void DrawAlgorithm(Graphics g, AlgorithmState st, Rectangle area)
        {
            var snap = st.Snapshot(); var data = snap.data;
            using var fill = new SolidBrush(ChartBg);
            using var border = new Pen(ChartGrid);
            g.FillRectangle(fill, area);
            g.DrawRectangle(border, area);
            using var titleFont = new Font("Consolas", 10F, FontStyle.Bold);
            using var infoFont = new Font("Consolas", 8F); using var valueFont = new Font("Consolas", 7.5F);
            TextRenderer.DrawText(g, st.Name, titleFont, new Rectangle(area.Left + U(13), area.Top + U(10), area.Width - U(40), U(23)),
                st.BarColor, TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, snap.finished ? snap.err == null ? "+" : "!" : ".", titleFont,
                new Point(area.Right - U(27), area.Top + U(10)), snap.err == null ? st.BarColor : SwapClr);
            string timing = snap.err != null ? "Ошибка: " + snap.err : snap.runs > 0
                ? $"≈ {FormatTime(snap.avg)}  ·  прогонов: {snap.runs}" : "Подготовка замера…";
            TextRenderer.DrawText(g, timing, infoFont, new Rectangle(area.Left + U(13), area.Top + U(34), area.Width - U(26), U(18)),
                CanvasMuted, TextFormatFlags.EndEllipsis);
            if (data.Length == 0) return;
            var plot = PlotBounds(area);
            for (int k = 1; k <= 3; k++) g.DrawLine(border, plot.Left, plot.Top + plot.Height * k / 3, plot.Right, plot.Top + plot.Height * k / 3);
            double max = data.Max(), min = data.Min(); if (max == min) max = min + 1;
            float barW = plot.Width / (float)data.Length;
            string format = "F" + _decimalPlaces;
            float valueWidth = Math.Max(
                g.MeasureString(min.ToString(format), valueFont).Width,
                g.MeasureString(max.ToString(format), valueFont).Width);
            bool showAll = barW >= valueWidth + U(3);
            float indexWidth = g.MeasureString(data.Length.ToString(), valueFont).Width + U(4);
            int step = Math.Max(1, (int)Math.Ceiling(indexWidth / barW));
            using var textBrush = new SolidBrush(CanvasText); using var dimBrush = new SolidBrush(CanvasMuted);
            for (int i = 0; i < data.Length; i++)
            {
                bool swap = i == snap.si || i == snap.sj, compare = i == snap.ci || i == snap.cj;
                Color color = swap ? SwapClr : compare ? CompareClr : st.BarColor;
                float height = Math.Max(U(2), (float)((data[i] - min + 0.5) / (max - min + 1)) * Math.Max(U(2), plot.Height - U(17)));
                float x = plot.Left + i * barW, y = plot.Bottom - height;
                var r = new RectangleF(x + 0.5F, y, Math.Max(1, barW - U(2)), height);
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, r);
                if (i == data.Length - 1 || (i % step == 0 && (data.Length - 1 - i) * barW >= indexWidth))
                {
                    string index = (i + 1).ToString(); var size = g.MeasureString(index, valueFont);
                    float ix = Math.Max(plot.Left, Math.Min(plot.Right - size.Width, x + (barW - size.Width) / 2));
                    g.DrawString(index, valueFont, dimBrush, ix, plot.Bottom + U(3));
                }
                if (showAll || ((swap || compare) && barW >= U(12)))
                {
                    string value = data[i].ToString(format); var size = g.MeasureString(value, valueFont);
                    float tx = Math.Max(plot.Left, Math.Min(plot.Right - size.Width, x + (barW - size.Width) / 2));
                    g.DrawString(value, valueFont, textBrush, tx, Math.Max(plot.Top, y - size.Height));
                }
            }
            long[] counts = { snap.cmp, snap.swp, snap.iter };
            string[] labels = { "сравнений", "обменов", "итераций" };
            for (int i = 0; i < 3; i++)
            {
                int x = area.Left + U(13) + i * (area.Width - U(26)) / 3, w = (area.Width - U(26)) / 3;
                TextRenderer.DrawText(g, counts[i].ToString("N0"), titleFont, new Rectangle(x, area.Bottom - U(38), w, U(20)),
                    CanvasText, TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, labels[i], infoFont, new Rectangle(x, area.Bottom - U(19), w, U(16)),
                    CanvasMuted, TextFormatFlags.EndEllipsis);
            }
        }

        private void ChartMouseMove(object? sender, MouseEventArgs e)
        {
            string text = "";
            foreach (var entry in _chartAreas)
            {
                var plot = PlotBounds(entry.area); if (!plot.Contains(e.Location)) continue;
                var data = entry.state.Snapshot().data; if (data.Length == 0) break;
                int index = Math.Min(data.Length - 1, (e.X - plot.Left) * data.Length / plot.Width);
                text = $"{entry.state.Name} · элемент №{index + 1}: {data[index].ToString("F" + _decimalPlaces)}"; break;
            }
            if (chartTip.GetToolTip(workspacePanel) != text) chartTip.SetToolTip(workspacePanel, text);
        }

        private void SetupGrid()
        {
            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Значение",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false,
                Visible = true
            });
            dataGrid.RowCount = 1;
        }

        private void DataGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 0) return;
            if (e.RowIndex < 0) return;

            var text = e.FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!TryParseDouble(text, out _))
            {
                MessageBox.Show(
                    $"Некорректное значение \"{text}\" в строке {e.RowIndex + 1}. Ожидается число.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private void DataGrid_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && e.RowIndex < _currentData.Length)
                e.Value = _currentData[e.RowIndex];
        }

        private void DataGrid_CellValuePushed(object? sender, DataGridViewCellValueEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0 || _isRunning) return;
            string text = e.Value?.ToString() ?? string.Empty;
            if (!TryParseDouble(text, out double value)) return;
            value = Math.Round(value, _decimalPlaces, MidpointRounding.AwayFromZero);

            if (e.RowIndex < _currentData.Length)
            {
                _currentData[e.RowIndex] = value;
                dataGrid.InvalidateRow(e.RowIndex);
                return;
            }

            if (e.RowIndex == _currentData.Length)
            {
                Array.Resize(ref _currentData, _currentData.Length + 1);
                _currentData[e.RowIndex] = value;
                if (dataGrid.IsHandleCreated)
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (IsDisposed || dataGrid.IsDisposed) return;
                        dataGrid.RowCount = _currentData.Length + 1;
                        SyncDataFromGrid();
                    }));
                }
            }
        }

        private void DeleteNumber(int index)
        {
            if (index < 0 || index >= _currentData.Length || _isRunning) return;
            dataGrid.CancelEdit();
            double[] result = new double[_currentData.Length - 1];
            if (index > 0) Array.Copy(_currentData, 0, result, 0, index);
            if (index < result.Length)
                Array.Copy(_currentData, index + 1, result, index, result.Length - index);
            _currentData = result;
            _hoveredDeleteRow = -1;
            dataGrid.RowCount = result.Length + 1;
            SyncDataFromGrid();
            dataGrid.Invalidate();
        }

        private void SyncDataFromGrid()
        {
            lblCount.Text = $"Элементов: {_currentData.Length:N0}";
            int numberWidth = Math.Max(U(42), TextRenderer.MeasureText(
                Math.Max(1, _currentData.Length).ToString(), dataGrid.Font).Width + U(23));
            if (dataGrid.RowHeadersWidth != numberWidth)
                dataGrid.RowHeadersWidth = numberWidth;
        }

        private void LoadDataToGrid(IEnumerable<double> data)
        {
            double[] values = data as double[] ?? data.ToArray();
            for (int i = 0; i < values.Length; i++)
                values[i] = Math.Round(values[i], _decimalPlaces, MidpointRounding.AwayFromZero);

            dataGrid.CancelEdit();
            _currentData = values;
            _hoveredDeleteRow = -1;
            dataGrid.RowCount = values.Length + 1;
            SyncDataFromGrid();
            dataGrid.Invalidate();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_isRunning) return;
            dataGrid.CancelEdit();
            _currentData = Array.Empty<double>();
            dataGrid.RowCount = 1;
            _hoveredDeleteRow = -1;
            _states.Clear(); _chartAreas.Clear(); reportBox.Clear();
            _showReport = false; btnReport.Enabled = false;
            SyncDataFromGrid();
            UpdateWorkspace();
            lblStatus.Text = "Данные очищены.";
        }

        private static SortingAlgorithm CreateAlgoByName(string name)
        {
            switch (name)
            {
                case "Пузырьковая": return new BubbleSort();
                case "Вставками":   return new InsertionSort();
                case "Шейкерная":   return new ShakerSort();
                case "Быстрая":     return new QuickSort();
                case "BOGO":        return new BogoSort();
                default: throw new ArgumentException("Неизвестный алгоритм: " + name);
            }
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            using var dlg = new GenerateForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var rnd = new Random();
            int count = (int)dlg.Count;
            var data = new double[count];
            for (int i = 0; i < count; i++)
            {
                double value = dlg.Min + rnd.NextDouble() * (dlg.Max - dlg.Min);
                data[i] = Math.Round(value, _decimalPlaces, MidpointRounding.AwayFromZero);
            }
            LoadDataToGrid(data);
        }

        private void BtnExcel_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel файлы (*.xlsx;*.xls)|*.xlsx;*.xls|Все файлы (*.*)|*.*",
                Title = "Выберите Excel-файл"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var data = LoadFromExcel(dlg.FileName);
                if (data.Count == 0)
                {
                    MessageBox.Show("Файл не содержит числовых данных.",
                        "Пустой файл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LoadDataToGrid(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки Excel: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<double> LoadFromExcel(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = new List<double>();

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            do
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 0) continue;
                    var val = reader.GetValue(0);
                    if (val == null) continue;
                    var s = val.ToString();
                    if (s == null) continue;

                    if (TryParseDouble(s, out double num))
                        result.Add(num);
                    else
                        throw new FormatException($"Значение \"{s}\" не является числом.");
                }
            } while (reader.NextResult());

            return result;
        }

        private async void BtnGoogle_Click(object? sender, EventArgs e)
        {
            using var dlg = new GoogleLinkForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var data = await LoadFromGoogleSheetsByLink(dlg.SheetUrl, dlg.UseHtml);
                if (data.Count == 0)
                {
                    MessageBox.Show("Таблица не содержит числовых данных.",
                        "Пустая таблица", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LoadDataToGrid(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки Google Sheets: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<double>> LoadFromGoogleSheetsByLink(string url, bool useHtml)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Пустая ссылка.");

            string spreadsheetId = ExtractSpreadsheetId(url);
            string gid = ExtractGid(url) ?? "0";

            string csvUrl = useHtml
                ? $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/gviz/tq?tqx=out:html&gid={gid}"
                : $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={gid}";

            string content;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                content = await client.GetStringAsync(csvUrl);
            }

            var result = new List<double>();

            if (useHtml)
            {
                var matches = Regex.Matches(content, @"<td[^>]*>(.*?)</td>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    var text = WebUtility.HtmlDecode(m.Groups[1].Value).Trim();
                    if (TryParseDouble(text, out double num))
                        result.Add(num);
                }
            }
            else
            {
                var lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var first = line.Split(',')[0].Trim().Trim('"');
                    if (TryParseDouble(first, out double num))
                        result.Add(num);
                }
            }

            return result;
        }

        private static string ExtractSpreadsheetId(string url)
        {
            var m = Regex.Match(url, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
            if (m.Success) return m.Groups[1].Value;
            throw new FormatException(
                "Не удалось извлечь ID таблицы из ссылки. Проверьте, что ссылка вида " +
                "https://docs.google.com/spreadsheets/d/XXXXXXXXX/edit");
        }

        private static string? ExtractGid(string url)
        {
            var m = Regex.Match(url, @"[#&]gid=(\d+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            lblStatus.Text = "Остановка...";
        }

        private async void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (_isRunning) return;

            if (dataGrid.IsCurrentCellInEditMode && !dataGrid.EndEdit()) return;
            SyncDataFromGrid();
            if (_currentData.Length == 0)
            {
                MessageBox.Show("Нет данных для сортировки.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbBogo.Checked && _currentData.Length > MaxBogoElements)
            {
                MessageBox.Show(
                    $"BOGO-сортировку нельзя запускать, если элементов больше {MaxBogoElements}.\n" +
                    $"Сейчас элементов: {_currentData.Length}.",
                    "Ограничение BOGO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedNames = new List<string>();
            if (cbBubble.Checked)    selectedNames.Add("Пузырьковая");
            if (cbInsertion.Checked) selectedNames.Add("Вставками");
            if (cbShaker.Checked)    selectedNames.Add("Шейкерная");
            if (cbQuick.Checked)     selectedNames.Add("Быстрая");
            if (cbBogo.Checked)      selectedNames.Add("BOGO");

            if (selectedNames.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один алгоритм сортировки.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isRunning = true;
            btnCalculate.Enabled = false;
            btnBackMenu.Enabled = false;
            btnStop.Enabled = true;
            btnClear.Enabled = false;
            leftPanel.Enabled = optionsPanel.Enabled = false;
            tbDelay.Enabled = false;
            nudDecimals.Enabled = false;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            bool asc = cmbDirection.SelectedIndex == 0;
            bool visualize = _currentData.Length <= MaxVisualizeElements;
            int delayMs = tbDelay.Value;

            lblStatus.Text = visualize
                ? $"Замер ({RunsPerAlgorithm} прогонов, кроме BOGO)..."
                : $"Сортировка выполняется (визуализация отключена, элементов > {MaxVisualizeElements}).";

            _states = new List<AlgorithmState>();
            foreach (var name in selectedNames)
            {
                var st = new AlgorithmState
                {
                    Name = name,
                    BarColor = _algoColors.TryGetValue(name, out var c) ? c : Accent,
                    Data = (double[])_currentData.Clone(),
                    Visualize = visualize && (name != "BOGO" || _currentData.Length <= 7)
                };
                _states.Add(st);
            }

            _showReport = false; reportBox.Clear(); btnReport.Enabled = false;
            UpdateWorkspace();
            if (_states.Any(state => state.Visualize)) _renderTimer.Start();

            var measureTasks = new List<Task>();

            foreach (var name in selectedNames)
            {
                var algoName = name;
                var st = _states.First(s => s.Name == algoName);
                bool isBogo = (algoName == "BOGO");
                int runs = isBogo ? 1 : RunsPerAlgorithm;

                measureTasks.Add(Task.Run(() =>
                {
                    var times = new List<double>(runs);
                    long firstCmp = 0, firstSwp = 0, firstIter = 0;
                    string? error = null;

                    for (int run = 0; run < runs; run++)
                    {
                        if (token.IsCancellationRequested) break;

                        var source = (double[])_currentData.Clone();
                        Array.Copy(source, st.Data, source.Length);

                        var algo = CreateAlgoByName(algoName);
                        algo.Ascending = asc;

                        bool collectThis = (run == 0);
                        long localCmp = 0, localSwp = 0;

                        if (collectThis && st.Visualize)
                        {
                            algo.OnCompare += (i, j) =>
                            {
                                System.Threading.Interlocked.Increment(ref localCmp);
                                lock (st.LogSync) st.EventLog.Add((0, i, j));
                            };
                            algo.OnSwap += (i, j) =>
                            {
                                System.Threading.Interlocked.Increment(ref localSwp);
                                lock (st.LogSync) st.EventLog.Add((1, i, j));
                            };
                        }
                        else if (collectThis)
                        {
                            algo.OnCompare += (i, j) => System.Threading.Interlocked.Increment(ref localCmp);
                            algo.OnSwap    += (i, j) => System.Threading.Interlocked.Increment(ref localSwp);
                        }

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        try { algo.Sort(st.Data); }
                        catch (Exception ex)
                        {
                            error = ex.Message;
                            sw.Stop();
                            break;
                        }
                        sw.Stop();

                        times.Add(sw.Elapsed.TotalMilliseconds);

                        if (collectThis)
                        {
                            firstCmp = localCmp;
                            firstSwp = localSwp;
                            firstIter = algo.Iterations;
                        }

                        lock (st.Sync) st.Runs = run + 1;
                    }

                    lock (st.Sync)
                    {
                        if (times.Count > 0)
                        {
                            st.AvgMs = times.Average();
                            st.MinMs = times.Min();
                            st.MaxMs = times.Max();
                        }
                        st.Comparisons = firstCmp;
                        st.Swaps = firstSwp;
                        st.Iterations = firstIter;
                        st.Error = error;
                    }
                }, token));
            }

            try { await Task.WhenAll(measureTasks); }
            catch (OperationCanceledException) { }

            if (visualize && !token.IsCancellationRequested)
            {
                lblStatus.Text = "Воспроизведение визуализации...";

                var playTasks = new List<Task>();

                foreach (var name in selectedNames)
                {
                    var st = _states.First(s => s.Name == name);
                    if (!st.Visualize)
                    {
                        lock (st.Sync) st.Finished = true;
                        continue;
                    }

                    playTasks.Add(Task.Run(() =>
                    {
                        var working = (double[])_currentData.Clone();
                        lock (st.Sync)
                        {
                            Array.Copy(working, st.Data, working.Length);
                            st.CompareI = st.CompareJ = st.SwapI = st.SwapJ = -1;
                        }

                        List<(int type, int i, int j)> log;
                        lock (st.LogSync) log = new List<(int, int, int)>(st.EventLog);

                        foreach (var ev in log)
                        {
                            if (token.IsCancellationRequested) break;

                            if (ev.type == 0)
                            {
                                lock (st.Sync)
                                {
                                    st.CompareI = ev.i;
                                    st.CompareJ = ev.j;
                                }
                            }
                            else
                            {
                                lock (st.Sync)
                                {
                                    if (ev.i >= 0 && ev.j >= 0 &&
                                        ev.i < st.Data.Length && ev.j < st.Data.Length)
                                    {
                                        (st.Data[ev.i], st.Data[ev.j]) = (st.Data[ev.j], st.Data[ev.i]);
                                    }
                                    st.SwapI = ev.i;
                                    st.SwapJ = ev.j;
                                }
                            }

                            if (delayMs > 0)
                            {
                                int slept = 0;
                                while (slept < delayMs && !token.IsCancellationRequested)
                                {
                                    int chunk = Math.Min(10, delayMs - slept);
                                    Thread.Sleep(chunk);
                                    slept += chunk;
                                }
                            }
                        }

                        lock (st.Sync)
                        {
                            st.CompareI = st.CompareJ = st.SwapI = st.SwapJ = -1;
                            st.Finished = true;
                        }
                    }, token));
                }

                try { await Task.WhenAll(playTasks); }
                catch (OperationCanceledException) { }
            }
            else
            {
                foreach (var st in _states)
                {
                    lock (st.Sync) st.Finished = true;
                }
            }

            var ordered = _states.OrderBy(s => s.AvgMs).ToList();
            var sb = new StringBuilder();

            if (token.IsCancellationRequested)
                sb.AppendLine("=== Результаты (остановлено пользователем) ===");
            else
                sb.AppendLine("=== Результаты ===");

            sb.AppendLine(
                $"{"Алгоритм",-16}{"Среднее",-20}{"Мин.",-20}{"Макс.",-20}" +
                $"{"Прогонов",10}{"Итерации",12}{"Сравн.",14}{"Обмены",12}");
            sb.AppendLine(new string('-', 124));

            foreach (var s in ordered)
            {
                var snap = s.Snapshot();
                if (snap.err != null)
                {
                    sb.AppendLine(
                        $"{s.Name,-16}{"ошибка",-20}{"",-20}{"",-20}" +
                        $"{snap.runs,10}{snap.iter,12}{snap.cmp,14}{snap.swp,12}  {snap.err}");
                }
                else
                {
                    sb.AppendLine(
                        $"{s.Name,-16}{FormatTime(snap.avg),-20}{FormatTime(snap.min),-20}{FormatTime(snap.max),-20}" +
                        $"{snap.runs,10}{snap.iter,12}{snap.cmp,14}{snap.swp,12}");
                }
            }

            var fastest = ordered.FirstOrDefault(s => s.Error == null && s.Runs > 0);
            if (fastest != null)
                sb.AppendLine($"\nСамый быстрый (по среднему): {fastest.Name} ({FormatTime(fastest.AvgMs)})");

            if (!visualize)
                sb.AppendLine($"\nВизуализация отключена: элементов больше {MaxVisualizeElements}.");
            else if (selectedNames.Contains("BOGO") && _currentData.Length > 7)
                sb.AppendLine("\nBOGO: воспроизведение отключено для массивов длиннее 7 элементов, сортировка и замер сохранены.");

            if (token.IsCancellationRequested)
                sb.AppendLine("\n*** Остановлено пользователем ***");

            reportBox.Text = sb.ToString();
            _showReport = !visualize; btnReport.Enabled = true;
            UpdateWorkspace();

            lblStatus.Text = token.IsCancellationRequested
                ? "Остановлено пользователем."
                : $"Готово. {_states.Count} алгоритм(ов). По {RunsPerAlgorithm} прогонов (BOGO — 1).";

            _renderTimer.Stop();
            _isRunning = false;
            btnCalculate.Enabled = true;
            btnBackMenu.Enabled = true;
            btnStop.Enabled = false;
            btnClear.Enabled = true;
            leftPanel.Enabled = optionsPanel.Enabled = true;
            tbDelay.Enabled = true;
            nudDecimals.Enabled = true;
            workspacePanel.Invalidate();
        }

        private void SortixForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_isRunning)
                {
                    _cts?.Cancel();
                    lblStatus.Text = "Остановка...";
                }
                else if (_isFullscreen) ExitFullscreen();
                else Close();
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (_isFullscreen) ExitFullscreen();
                else EnterFullscreen();
            }
        }
    }

    public class GenerateForm : Form
    {
        private NumericUpDown nudCount = null!, nudMin = null!, nudMax = null!;
        public double Count => (double)nudCount.Value;
        public double Min => (double)nudMin.Value;
        public double Max => (double)nudMax.Value;

        public GenerateForm()
        {
            var body = SortixForm.CreateDialogBody(this, "Генерация данных", new Size(320, 208), 2);
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, RowCount = 3, Margin = Padding.Empty };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            nudCount = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 15 };
            nudMin = new NumericUpDown { Minimum = -1000000, Maximum = 1000000, Value = 0,
                DecimalPlaces = 3, Increment = 0.1m };
            nudMax = new NumericUpDown { Minimum = -1000000, Maximum = 1000000, Value = 100,
                DecimalPlaces = 3, Increment = 0.1m };
            var numbers = new[] { nudCount, nudMin, nudMax };
            string[] titles = { "Количество", "Минимум", "Максимум" };
            for (int i = 0; i < numbers.Length; i++)
            {
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var label = new Label { Text = titles[i], AutoSize = true, Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 5, 12, 5) };
                numbers[i].Width = 144; numbers[i].Anchor = AnchorStyles.Right;
                numbers[i].Margin = new Padding(0, 6, 0, 6);
                numbers[i].AccessibleName = titles[i]; numbers[i].TabIndex = i;
                fields.Controls.Add(label, 0, i); fields.Controls.Add(numbers[i], 1, i);
            }
            body.Controls.Add(fields, 0, 0);
            var btnOk = new Button { Text = "Создать", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
            btnOk.Click += (s, e) =>
            {
                if (nudMin.Value >= nudMax.Value)
                {
                    MessageBox.Show(this, "Минимум должен быть меньше максимума.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };
            body.Controls.Add(SortixForm.CreateDialogActions(btnOk, btnCancel), 0, 1);
            AcceptButton = btnOk; CancelButton = btnCancel;
            SortixForm.ApplyDialogStyle(this);
            ResumeLayout(true);
        }
    }

    public class GoogleLinkForm : Form
    {
        private TextBox txtUrl = null!;
        private CheckBox chkHtml = null!;
        public string SheetUrl => txtUrl.Text.Trim();
        public bool UseHtml => chkHtml.Checked;

        public GoogleLinkForm()
        {
            var body = SortixForm.CreateDialogBody(this, "Импорт из Google Sheets", new Size(580, 250), 6);
            for (int i = 0; i < 6; i++)
                body.RowStyles.Add(new RowStyle(i == 4 ? SizeType.Percent : SizeType.AutoSize, i == 4 ? 100 : 0));
            var intro = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10),
                Text = "Вставьте ссылку на Google-таблицу.\nТаблица должна быть опубликована или доступна по ссылке." };
            txtUrl = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 10),
                Text = "https://docs.google.com/spreadsheets/d/", Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.FixedSingle, AccessibleName = "Ссылка на Google-таблицу", TabIndex = 0 };
            chkHtml = new CheckBox { AutoSize = true, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                Text = "Использовать HTML-формат (если CSV не работает)", Margin = new Padding(0, 6, 0, 10), TabIndex = 1 };
            var hint = new Label { AutoSize = true, Dock = DockStyle.Fill, Tag = "muted", Margin = Padding.Empty,
                Font = new Font("Consolas", 8.5F), Text = "Пример: https://docs.google.com/spreadsheets/d/1AbCdEf123.../edit#gid=0" };
            body.Controls.Add(intro, 0, 0); body.Controls.Add(txtUrl, 0, 1);
            body.Controls.Add(chkHtml, 0, 2); body.Controls.Add(hint, 0, 3);
            var btnOk = new Button { Text = "Загрузить", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SheetUrl))
                {
                    MessageBox.Show(this, "Вставьте ссылку на таблицу.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                var m = Regex.Match(SheetUrl, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
                if (!m.Success)
                {
                    MessageBox.Show(this,
                        "Не удалось найти ID таблицы в ссылке.\n\n" +
                        "Убедитесь, что ссылка вида:\nhttps://docs.google.com/spreadsheets/d/XXXXXXXXX/edit",
                        "Неверная ссылка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };
            body.Controls.Add(SortixForm.CreateDialogActions(btnOk, btnCancel), 0, 5);
            AcceptButton = btnOk; CancelButton = btnCancel;
            SortixForm.ApplyDialogStyle(this);
            ResumeLayout(true);
        }
    }
}