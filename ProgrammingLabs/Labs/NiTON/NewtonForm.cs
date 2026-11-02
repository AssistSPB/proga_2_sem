using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ProgrammingLabs.Labs.Newton
{
    public partial class NewtonForm : Form
    {
        private static readonly Color Background = Color.FromArgb(49, 53, 39);
        private static readonly Color Surface = Color.FromArgb(60, 65, 48);
        private static readonly Color Foreground = Color.FromArgb(215, 215, 190);
        private static readonly Color Accent = Color.FromArgb(172, 185, 132);
        private static readonly Color GridColor = Color.FromArgb(83, 91, 70);
        private static readonly Color Marker = Color.FromArgb(239, 177, 91);

        private readonly TextBox formulaBox = new TextBox();
        private readonly TextBox aBox = new TextBox();
        private readonly TextBox bBox = new TextBox();
        private readonly TextBox precisionBox = new TextBox();
        private readonly TextBox resultBox = new TextBox();
        private readonly RadioButton minMode = new RadioButton();
        private readonly RadioButton maxMode = new RadioButton();
        private readonly Label resultTitle = Caption("Минимум", 10, true);
        private readonly PlotPanel plot = new PlotPanel();
        private readonly List<(double X, double Y)> steps = new List<(double X, double Y)>();
        private ToolStripMenuItem previousItem = null!;
        private ToolStripMenuItem nextItem = null!;
        private ToolStripMenuItem allItem = null!;
        private const int Samples = 1000;
        private readonly double[] graphY = new double[Samples + 1];
        private readonly bool[] graphValid = new bool[Samples + 1];
        private ExpressionParser? function;
        private double start, end, minGraphY, maxGraphY, tolerance;
        private int selectedStep;

        public NewtonForm()
        {
            InitializeComponent();
            BuildInterface();
        }

        private static Label Caption(string text, float size = 9, bool bold = false) => new Label
        {
            Text = text, Dock = DockStyle.Fill, ForeColor = Accent,
            Font = new Font("Tahoma", size, bold ? FontStyle.Bold : FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft
        };

        private static void StyleInput(TextBox box, string text)
        {
            box.Dock = DockStyle.Fill;
            box.Text = text;
            box.BackColor = Color.FromArgb(46, 51, 39);
            box.ForeColor = Foreground;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = new Font("Consolas", 11F);
        }

        private static void SetupMode(RadioButton button, string text, bool selected)
        {
            button.Text = text;
            button.Appearance = Appearance.Button;
            button.AutoSize = false;
            button.Size = new Size(116, 27);
            button.Margin = new Padding(0, 0, 7, 0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = GridColor;
            button.FlatAppearance.CheckedBackColor = Accent;
            button.Font = new Font("Tahoma", 9F);
            button.ForeColor = Foreground;
            button.BackColor = Background;
            button.Checked = selected;
            button.Cursor = Cursors.Hand;
        }

        private void BuildInterface()
        {
            SuspendLayout();
            Text = "Newton";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 560);
            Size = new Size(1120, 690);
            BackColor = Background;
            Font = new Font("Tahoma", 9F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1,
                Padding = new Padding(10), BackColor = Background
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2,
                BackColor = Surface, Margin = new Padding(0, 0, 0, 6)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 490));
            header.Controls.Add(Caption("  Console  /  Newton", 12, true), 0, 0);

            var menu = new MenuStrip
            {
                Dock = DockStyle.Fill, AutoSize = false,
                BackColor = Surface, Font = new Font("Tahoma", 9F),
                Padding = new Padding(0, 6, 0, 5)
            };
            var calculate = new ToolStripMenuItem("Рассчитать")
            {
                ForeColor = Accent, ShortcutKeys = Keys.Control | Keys.Enter,
                ShowShortcutKeys = false
            };
            var clear = new ToolStripMenuItem("Очистить") { ForeColor = Accent };
            previousItem = new ToolStripMenuItem("Назад") { ForeColor = Accent, Enabled = false };
            nextItem = new ToolStripMenuItem("Шаг") { ForeColor = Accent, Enabled = false };
            allItem = new ToolStripMenuItem("Всё") { ForeColor = Accent, Enabled = false };
            var back = new ToolStripMenuItem("В меню") { ForeColor = Accent };
            calculate.Click += (_, _) => Calculate();
            clear.Click += (_, _) => Clear();
            previousItem.Click += (_, _) => ShowStep(selectedStep - 1);
            nextItem.Click += (_, _) => ShowStep(selectedStep + 1);
            allItem.Click += (_, _) => ShowStep(steps.Count - 1);
            back.Click += (_, _) => Close();
            menu.Items.AddRange(new ToolStripItem[]
                { calculate, clear, previousItem, nextItem, allItem, back });
            MainMenuStrip = menu;
            header.Controls.Add(menu, 1, 0);
            layout.Controls.Add(header, 0, 0);

            var inputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4,
                Padding = new Padding(12, 7, 12, 5), BackColor = Surface,
                Margin = new Padding(0, 0, 0, 7)
            };
            foreach (float percent in new[] { 42f, 19f, 19f, 20f })
                inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percent));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var boxes = new[] { formulaBox, aBox, bBox, precisionBox };
            var labels = new[] { "f(x)", "a", "b", "ε" };
            var defaults = new[] { "", "", "", "0,0001" };
            for (int i = 0; i < boxes.Length; i++)
            {
                inputs.Controls.Add(Caption(labels[i]), i, 0);
                StyleInput(boxes[i], defaults[i]);
                boxes[i].Margin = new Padding(0, 0, 8, 0);
                inputs.Controls.Add(boxes[i], i, 1);
            }
            var modes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            var modeCaption = Caption("РЕЖИМ", 8, true);
            modeCaption.Dock = DockStyle.None;
            modeCaption.Width = 74;
            modeCaption.Height = 27;
            modes.Controls.Add(modeCaption);
            SetupMode(minMode, "Минимум", true);
            SetupMode(maxMode, "Максимум", false);
            modes.Controls.Add(minMode);
            modes.Controls.Add(maxMode);
            minMode.CheckedChanged += (_, _) => UpdateMode();
            maxMode.CheckedChanged += (_, _) => UpdateMode();
            inputs.Controls.Add(modes, 0, 2);
            inputs.SetColumnSpan(modes, 4);
            var hint = Caption("x^2, (x-2)^2, sin(x), cos(x), sqrt(x), ln(x)", 8);
            inputs.Controls.Add(hint, 0, 3);
            inputs.SetColumnSpan(hint, 4);
            layout.Controls.Add(inputs, 0, 1);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            var graphPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Background,
                Padding = new Padding(10), Margin = new Padding(0, 0, 8, 0)
            };
            var graphTitle = Caption("График функции", 10, true);
            graphTitle.Dock = DockStyle.Top;
            graphTitle.Height = 27;
            plot.Dock = DockStyle.Fill;
            plot.BackColor = Background;
            plot.Paint += DrawGraph;
            graphPanel.Controls.Add(plot);
            graphPanel.Controls.Add(graphTitle);

            var resultPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(10)
            };
            resultTitle.Dock = DockStyle.Top;
            resultTitle.Height = 28;
            resultBox.Dock = DockStyle.Fill;
            resultBox.ReadOnly = true;
            resultBox.Multiline = true;
            resultBox.ScrollBars = ScrollBars.Vertical;
            resultBox.BorderStyle = BorderStyle.None;
            resultBox.BackColor = Surface;
            resultBox.ForeColor = Foreground;
            resultBox.Font = new Font("Consolas", 10);
            resultBox.Text = "Нет расчёта";
            resultPanel.Controls.Add(resultBox);
            resultPanel.Controls.Add(resultTitle);
            content.Controls.Add(graphPanel, 0, 0);
            content.Controls.Add(resultPanel, 1, 0);
            layout.Controls.Add(content, 0, 2);
            Controls.Add(layout);
            UpdateMode();
            ResumeLayout(false);
        }

        private void UpdateMode()
        {
            if (!minMode.Checked && !maxMode.Checked) return;
            minMode.BackColor = minMode.Checked ? Accent : Background;
            minMode.ForeColor = minMode.Checked ? Background : Foreground;
            maxMode.BackColor = maxMode.Checked ? Accent : Background;
            maxMode.ForeColor = maxMode.Checked ? Background : Foreground;
            resultTitle.Text = minMode.Checked ? "Минимум" : "Максимум";
            ResetResult();
        }

        private void ResetResult()
        {
            function = null;
            steps.Clear();
            selectedStep = 0;
            resultBox.Text = "Нет расчёта";
            previousItem.Enabled = false;
            nextItem.Enabled = false;
            allItem.Enabled = false;
            plot.Invalidate();
        }

        private void Clear()
        {
            formulaBox.Clear();
            aBox.Clear();
            bBox.Clear();
            precisionBox.Text = "0,0001";
            ResetResult();
            formulaBox.Focus();
        }

        private static double ReadNumber(string text, string name)
        {
            if (!double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double value) ||
                double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException($"Параметр {name} должен быть вещественным числом.");
            return value;
        }

        private static (double Slope, double Curvature) Derivatives(
            ExpressionParser f, double x, double a, double b)
        {
            double h = Math.Min(0.0005 * Math.Max(1, Math.Abs(x)),
                0.4 * Math.Min(x - a, b - x));
            if (h <= 1e-9 * Math.Max(1, Math.Abs(x)))
                throw new ArithmeticException("Слишком близко к границе интервала.");
            double y = f.Calculate(x);
            double minus = f.Calculate(x - h);
            double plus = f.Calculate(x + h);
            double slope = (plus - minus) / (2 * h);
            double curvature = ((plus - y) + (minus - y)) / (h * h);
            if (double.IsNaN(slope) || double.IsNaN(curvature) ||
                double.IsInfinity(slope) || double.IsInfinity(curvature))
                throw new ArithmeticException("Не удалось вычислить производные.");
            return (slope, curvature);
        }

        private static bool HasCorrectExtremumType(
            ExpressionParser f, double x, double a, double b, double eps, bool findMin)
        {
            double margin = Math.Min(x - a, b - x);
            double step = Math.Min(margin * 0.45,
                Math.Max(8 * eps, (b - a) * 0.0001));
            if (step <= 1e-9 * Math.Max(1, Math.Abs(x))) return false;
            double y = f.Calculate(x);
            double left = f.Calculate(x - step);
            double right = f.Calculate(x + step);
            return findMin ? left >= y && right >= y : left <= y && right <= y;
        }

        private List<(double X, double Y)> Newton(
            ExpressionParser f, double a, double b, double eps, double seed, bool findMin)
        {
            double x = seed;
            double sign = findMin ? 1 : -1;
            var track = new List<(double X, double Y)> { (x, f.Calculate(x)) };
            for (int i = 0; i < 100; i++)
            {
                var (slope, curvature) = Derivatives(f, x, a, b);
                if (sign * curvature <= 1e-12)
                    throw new ArithmeticException("Неподходящая кривизна для выбранного режима.");
                double delta = slope / curvature;
                if (!double.IsFinite(delta))
                    throw new ArithmeticException("Неустойчивый шаг Ньютона.");
                if (Math.Abs(delta) <= eps)
                {
                    if (!HasCorrectExtremumType(f, x, a, b, eps, findMin))
                        throw new ArithmeticException("Экстремум выбранного типа не подтверждён.");
                    return track;
                }

                double alpha = 1;
                double next = x;
                double nextY = track[track.Count - 1].Y;
                bool accepted = false;
                for (int j = 0; j < 40; j++)
                {
                    double candidate = x - alpha * delta;
                    if (candidate > a && candidate < b)
                    {
                        double candidateY = f.Calculate(candidate);
                        if (sign * (candidateY - nextY) < 0)
                        {
                            next = candidate;
                            nextY = candidateY;
                            accepted = true;
                            break;
                        }
                    }
                    alpha *= 0.5;
                }
                if (!accepted)
                    throw new ArithmeticException("Метод Ньютона не смог улучшить значение.");
                x = next;
                track.Add((x, nextY));
            }
            throw new ArithmeticException("Метод Ньютона не сошёлся за 100 шагов.");
        }

        private void Calculate()
        {
            try
            {
                string expression = formulaBox.Text.Trim();
                if (expression.Length == 0)
                    throw new ArgumentException("Введите формулу f(x).");
                if (expression.StartsWith("f(x)", StringComparison.OrdinalIgnoreCase) &&
                    expression.Contains('='))
                    expression = expression.Substring(expression.IndexOf('=') + 1).Trim();
                double a = ReadNumber(aBox.Text, "a");
                double b = ReadNumber(bBox.Text, "b");
                double eps = ReadNumber(precisionBox.Text, "ε");
                if (a >= b) throw new ArgumentException("Нужно a < b.");
                if (eps <= 0) throw new ArgumentException("Точность ε должна быть больше нуля.");
                if (!double.IsFinite(b - a))
                    throw new ArgumentException("Слишком большой интервал.");

                var f = new ExpressionParser(expression);
                f.Calculate(a);
                f.Calculate(b);
                bool findMin = minMode.Checked;
                double[] fractions = { 0.4, 0.5, 0.25, 0.75, 0.1, 0.9 };
                List<(double X, double Y)>? found = null;
                foreach (double fraction in fractions)
                {
                    double initial = a + (b - a) * fraction;
                    try
                    {
                        found = Newton(f, a, b, eps, initial, findMin);
                        break;
                    }
                    catch (ArithmeticException) { }
                }
                if (found == null)
                    throw new ArithmeticException("Не найден внутренний экстремум выбранного типа. Измените интервал.");

                steps.Clear();
                steps.AddRange(found);
                function = f;
                start = a;
                end = b;
                tolerance = eps;
                selectedStep = 0;
                PrepareGraph();
                ShowStep(0);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException ||
                                       ex is ArithmeticException || ex is OverflowException)
            {
                ResetResult();
                resultBox.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowStep(int index)
        {
            if (steps.Count == 0) return;
            selectedStep = Math.Max(0, Math.Min(index, steps.Count - 1));
            previousItem.Enabled = selectedStep > 0;
            nextItem.Enabled = selectedStep < steps.Count - 1;
            allItem.Enabled = selectedStep < steps.Count - 1;
            var current = steps[selectedStep];
            var final = steps[steps.Count - 1];
            int decimals = tolerance >= 1 ? 0 :
                (int)Math.Min(12, Math.Max(0, Math.Ceiling(-Math.Log10(tolerance))));
            resultBox.Text =
                $"x {(minMode.Checked ? "min" : "max")} ≈ {final.X.ToString("F" + decimals, CultureInfo.CurrentCulture)}\r\n" +
                $"f(x) ≈ {final.Y.ToString("G12", CultureInfo.CurrentCulture)}\r\n\r\n" +
                $"Шаг {selectedStep} / {steps.Count - 1}\r\n" +
                $"x = {current.X.ToString("G12", CultureInfo.CurrentCulture)}\r\n" +
                $"f(x) = {current.Y.ToString("G12", CultureInfo.CurrentCulture)}\r\n\r\n" +
                $"ε = {tolerance:G8}";
            plot.Invalidate();
        }

        private void PrepareGraph()
        {
            if (function == null) return;
            var values = new List<double>(Samples + 1);
            for (int i = 0; i <= Samples; i++)
            {
                double x = start + (end - start) * i / Samples;
                try
                {
                    double y = function.Calculate(x);
                    graphY[i] = y;
                    graphValid[i] = true;
                    values.Add(y);
                }
                catch (ArithmeticException) { graphValid[i] = false; }
                catch (FormatException) { graphValid[i] = false; }
            }
            if (values.Count == 0)
            {
                minGraphY = -1;
                maxGraphY = 1;
                return;
            }
            values.Sort();
            minGraphY = values[Math.Min(values.Count - 1, values.Count / 50)];
            maxGraphY = values[Math.Max(0, values.Count - 1 - values.Count / 50)];
            foreach (var point in steps)
            {
                minGraphY = Math.Min(minGraphY, point.Y);
                maxGraphY = Math.Max(maxGraphY, point.Y);
            }
            double span = maxGraphY - minGraphY;
            if (span <= 1e-12 || double.IsInfinity(span)) span = 1;
            minGraphY -= span * 0.1;
            maxGraphY += span * 0.1;
        }

        private void DrawGraph(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = plot.ClientRectangle;
            if (function == null || steps.Count == 0)
            {
                TextRenderer.DrawText(g, "После расчёта здесь появится график",
                    Font, bounds, Accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            Rectangle r = new Rectangle(67, 23, bounds.Width - 95, bounds.Height - 72);
            if (r.Width < 80 || r.Height < 80) return;
            double span = maxGraphY - minGraphY;
            using var grid = new Pen(GridColor);
            using var axes = new Pen(Color.FromArgb(140, 151, 113));
            using var curve = new Pen(Accent, 2f);
            using var marker = new Pen(Marker, 1.3f) { DashStyle = DashStyle.Dash };
            using var brush = new SolidBrush(Marker);
            using var trail = new SolidBrush(Color.FromArgb(155, 164, 124));
            for (int i = 0; i <= 5; i++)
            {
                int xx = r.Left + r.Width * i / 5;
                int yy = r.Top + r.Height * i / 5;
                g.DrawLine(grid, xx, r.Top, xx, r.Bottom);
                g.DrawLine(grid, r.Left, yy, r.Right, yy);
            }
            if (start <= 0 && end >= 0)
            {
                float xx = r.Left + (float)((0 - start) / (end - start) * r.Width);
                g.DrawLine(axes, xx, r.Top, xx, r.Bottom);
            }
            if (minGraphY <= 0 && maxGraphY >= 0)
            {
                float yy = r.Bottom - (float)((0 - minGraphY) / span * r.Height);
                g.DrawLine(axes, r.Left, yy, r.Right, yy);
            }
            var saved = g.Save();
            try
            {
                g.SetClip(r);
                for (int i = 1; i <= Samples; i++)
                {
                    if (!graphValid[i - 1] || !graphValid[i]) continue;
                    double y0 = graphY[i - 1], y1 = graphY[i];
                    if ((y0 < minGraphY && y1 > maxGraphY) ||
                        (y1 < minGraphY && y0 > maxGraphY)) continue;
                    float py0 = r.Bottom - (float)((y0 - minGraphY) / span * r.Height);
                    float py1 = r.Bottom - (float)((y1 - minGraphY) / span * r.Height);
                    if (Math.Abs(py0) > 100000 || Math.Abs(py1) > 100000) continue;
                    float px0 = r.Left + r.Width * (i - 1) / (float)Samples;
                    float px1 = r.Left + r.Width * i / (float)Samples;
                    g.DrawLine(curve, px0, py0, px1, py1);
                }
                for (int i = 0; i < selectedStep; i++)
                {
                    float px0 = r.Left + (float)((steps[i].X - start) / (end - start) * r.Width);
                    float py0 = r.Bottom - (float)((steps[i].Y - minGraphY) / span * r.Height);
                    float px1 = r.Left + (float)((steps[i + 1].X - start) / (end - start) * r.Width);
                    float py1 = r.Bottom - (float)((steps[i + 1].Y - minGraphY) / span * r.Height);
                    g.DrawLine(marker, px0, py0, px1, py1);
                    g.FillEllipse(trail, px0 - 3, py0 - 3, 6, 6);
                }
                var current = steps[selectedStep];
                float x = r.Left + (float)((current.X - start) / (end - start) * r.Width);
                float y = r.Bottom - (float)((current.Y - minGraphY) / span * r.Height);
                g.DrawLine(marker, x, r.Top, x, r.Bottom);
                g.FillEllipse(brush, x - 6, y - 6, 12, 12);
            }
            finally { g.Restore(saved); }
            using var labels = new Font("Consolas", 9f);
            using var labelBrush = new SolidBrush(Foreground);
            g.DrawString(start.ToString("G5"), labels, labelBrush, r.Left, r.Bottom + 6);
            string bLabel = end.ToString("G5");
            g.DrawString(bLabel, labels, labelBrush,
                r.Right - g.MeasureString(bLabel, labels).Width, r.Bottom + 6);
            g.DrawString(maxGraphY.ToString("G4"), labels, labelBrush, 2, r.Top);
            g.DrawString(minGraphY.ToString("G4"), labels, labelBrush, 2, r.Bottom - 16);
        }

        private sealed class PlotPanel : Panel
        {
            public PlotPanel() { DoubleBuffered = true; ResizeRedraw = true; }
        }

        private sealed class ExpressionParser
        {
            private readonly string text;
            private int position;
            private double x;

            public ExpressionParser(string formula)
            {
                text = formula;
            }

            public double Calculate(double argument)
            {
                x = argument;
                position = 0;
                double result = ReadExpression();
                SkipWhitespace();
                if (position != text.Length)
                    throw new FormatException("Лишний символ в формуле: «" +
                        text.Substring(position) + "».");
                if (double.IsNaN(result) || double.IsInfinity(result))
                    throw new ArithmeticException("Функция не определена при x = " +
                        argument.ToString("G10", CultureInfo.CurrentCulture) + ".");
                return result;
            }

            private void SkipWhitespace()
            {
                while (position < text.Length && char.IsWhiteSpace(text[position]))
                    position++;
            }

            private bool Accept(char symbol)
            {
                SkipWhitespace();
                if (position < text.Length && text[position] == symbol)
                {
                    position++;
                    return true;
                }
                return false;
            }

            private double ReadExpression()
            {
                double value = ReadTerm();
                while (true)
                {
                    if (Accept('+')) value += ReadTerm();
                    else if (Accept('-')) value -= ReadTerm();
                    else return value;
                }
            }

            private double ReadTerm()
            {
                double value = ReadUnary();
                while (true)
                {
                    if (Accept('*')) value *= ReadUnary();
                    else if (Accept('/')) value /= ReadUnary();
                    else return value;
                }
            }

            private double ReadUnary()
            {
                if (Accept('+')) return ReadUnary();
                if (Accept('-')) return -ReadUnary();
                return ReadPower();
            }

            private double ReadPower()
            {
                double value = ReadAtom();
                if (Accept('^')) value = Math.Pow(value, ReadUnary());
                return value;
            }

            private double ReadAtom()
            {
                SkipWhitespace();
                if (Accept('('))
                {
                    double inner = ReadExpression();
                    if (!Accept(')'))
                        throw new FormatException("Не хватает закрывающей скобки ).");
                    return inner;
                }

                if (position < text.Length &&
                    (char.IsDigit(text[position]) || text[position] == '.' || text[position] == ','))
                    return ReadNumber();

                if (position < text.Length && char.IsLetter(text[position]))
                {
                    int start = position;
                    while (position < text.Length && char.IsLetter(text[position]))
                        position++;
                    string name = text.Substring(start, position - start).ToLowerInvariant();
                    if (name == "x") return x;
                    if (name == "pi" || name == "π") return Math.PI;
                    if (name == "e") return Math.E;
                    if (!Accept('('))
                        throw new FormatException("После «" + name + "» нужны скобки, например sin(x).");
                    double argument = ReadExpression();
                    if (!Accept(')'))
                        throw new FormatException("Не закрыта скобка у функции «" + name + "».");
                    switch (name)
                    {
                        case "sin": return Math.Sin(argument);
                        case "cos": return Math.Cos(argument);
                        case "tan":
                        case "tg": return Math.Tan(argument);
                        case "sqrt": return Math.Sqrt(argument);
                        case "ln": return Math.Log(argument);
                        case "log": return Math.Log10(argument);
                        case "exp": return Math.Exp(argument);
                        case "abs": return Math.Abs(argument);
                        default: throw new FormatException("Неизвестная функция «" + name + "».");
                    }
                }
                throw new FormatException("Неверная формула около позиции " + (position + 1) + ".");
            }

            private double ReadNumber()
            {
                int start = position;
                bool gotDigits = false;
                while (position < text.Length && char.IsDigit(text[position]))
                {
                    gotDigits = true;
                    position++;
                }
                if (position < text.Length && (text[position] == '.' || text[position] == ','))
                {
                    position++;
                    while (position < text.Length && char.IsDigit(text[position]))
                    {
                        gotDigits = true;
                        position++;
                    }
                }
                if (!gotDigits)
                    throw new FormatException("Неверное число в формуле.");
                if (position < text.Length && (text[position] == 'e' || text[position] == 'E'))
                {
                    int exponentMark = position;
                    position++;
                    if (position < text.Length && (text[position] == '+' || text[position] == '-'))
                        position++;
                    int exponentStart = position;
                    while (position < text.Length && char.IsDigit(text[position]))
                        position++;
                    if (position == exponentStart) position = exponentMark;
                }
                string number = text.Substring(start, position - start).Replace(',', '.');
                if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double result))
                    throw new FormatException("Не удалось прочитать число «" + number + "».");
                return result;
            }
        }

    }
}