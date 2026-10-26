using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ProgrammingLabs.Labs.GoldenSection
{
    public partial class GoldenSectionForm : Form
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
        private readonly PlotPanel plot = new PlotPanel();
        private const int Samples = 1000;
        private readonly double[] graphY = new double[Samples + 1];
        private readonly bool[] graphValid = new bool[Samples + 1];
        private ExpressionParser? function;
        private double start, end, minimumX, minimumY, minGraphY, maxGraphY;

        public GoldenSectionForm()
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

        private void BuildInterface()
        {
            SuspendLayout();
            Text = "Golden Section";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(860, 550);
            Size = new Size(1120, 690);
            BackColor = Background;
            Font = new Font("Tahoma", 9F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1,
                Padding = new Padding(10), BackColor = Background
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2,
                BackColor = Surface, Margin = new Padding(0, 0, 0, 6) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            header.Controls.Add(Caption("  Console  /  Golden Section", 12, true), 0, 0);
            var menu = new MenuStrip { Dock = DockStyle.Fill, AutoSize = false,
                BackColor = Surface, Font = new Font("Tahoma", 9F), Padding = new Padding(0, 6, 0, 5) };
            var calculate = new ToolStripMenuItem("Рассчитать")
                { ForeColor = Accent, ShortcutKeys = Keys.Control | Keys.Enter, ShowShortcutKeys = false };
            var clear = new ToolStripMenuItem("Очистить") { ForeColor = Accent };
            var back = new ToolStripMenuItem("В меню") { ForeColor = Accent };
            calculate.Click += (_, _) => Calculate();
            clear.Click += (_, _) => Clear();
            back.Click += (_, _) => Close();
            menu.Items.AddRange(new ToolStripItem[] { calculate, clear, back });
            MainMenuStrip = menu;
            header.Controls.Add(menu, 1, 0);
            layout.Controls.Add(header, 0, 0);

            var inputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3,
                Padding = new Padding(12, 7, 12, 5), BackColor = Surface,
                Margin = new Padding(0, 0, 0, 7)
            };
            foreach (float percent in new[] { 42f, 19f, 19f, 20f })
                inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percent));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
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
            var hint = Caption("Минимум на [a; b]    •    x^2, sin(x), cos(x), sqrt(x), ln(x), abs(x)", 8);
            inputs.Controls.Add(hint, 0, 2);
            inputs.SetColumnSpan(hint, 4);
            layout.Controls.Add(inputs, 0, 1);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            var graphPanel = new Panel { Dock = DockStyle.Fill, BackColor = Background,
                Padding = new Padding(10), Margin = new Padding(0, 0, 8, 0) };
            var graphTitle = Caption("График функции", 10, true);
            graphTitle.Dock = DockStyle.Top;
            graphTitle.Height = 27;
            plot.Dock = DockStyle.Fill;
            plot.BackColor = Background;
            plot.Paint += DrawGraph;
            graphPanel.Controls.Add(plot);
            graphPanel.Controls.Add(graphTitle);

            var resultPanel = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(10) };
            var resultTitle = Caption("Минимум", 10, true);
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
            ResumeLayout(false);
        }

        private void Clear()
        {
            formulaBox.Clear();
            aBox.Clear();
            bBox.Clear();
            precisionBox.Text = "0,0001";
            resultBox.Text = "Нет расчёта";
            function = null;
            plot.Invalidate();
            formulaBox.Focus();
        }

        private static double ReadNumber(string text, string name)
        {
            if (!double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double number) ||
                double.IsNaN(number) || double.IsInfinity(number))
                throw new ArgumentException($"Параметр {name} должен быть вещественным числом.");
            return number;
        }

        private void Calculate()
        {
            try
            {
                string text = formulaBox.Text.Trim();
                if (text.Length == 0) throw new ArgumentException("Введите формулу f(x).");
                if (text.StartsWith("f(x)", StringComparison.OrdinalIgnoreCase) && text.Contains('='))
                    text = text.Substring(text.IndexOf('=') + 1).Trim();
                double a = ReadNumber(aBox.Text, "a");
                double b = ReadNumber(bBox.Text, "b");
                double epsilon = ReadNumber(precisionBox.Text, "ε");
                if (a >= b) throw new ArgumentException("Нужно a < b.");
                if (epsilon <= 0) throw new ArgumentException("Точность ε должна быть больше нуля.");
                if (double.IsInfinity(b - a)) throw new ArgumentException("Слишком большой интервал.");

                var f = new ExpressionParser(text);
                f.Calculate(a);
                f.Calculate(b);
                const double ratio = 0.6180339887498948482;
                double left = a, right = b;
                double x1 = right - ratio * (right - left);
                double x2 = left + ratio * (right - left);
                double y1 = f.Calculate(x1), y2 = f.Calculate(x2);
                int iterations = 0;

                while (right - left > epsilon && iterations < 2048)
                {
                    double previous = right - left;
                    if (y1 <= y2)
                    {
                        right = x2;
                        x2 = x1;
                        y2 = y1;
                        x1 = right - ratio * (right - left);
                        y1 = f.Calculate(x1);
                    }
                    else
                    {
                        left = x1;
                        x1 = x2;
                        y1 = y2;
                        x2 = left + ratio * (right - left);
                        y2 = f.Calculate(x2);
                    }
                    iterations++;
                    if (right - left >= previous) break;
                }
                if (right - left > epsilon)
                    throw new ArithmeticException("Не удалось достичь заданной точности.");

                double x = left + (right - left) / 2;
                double y = f.Calculate(x);
                int decimals = epsilon >= 1 ? 0 :
                    (int)Math.Min(12, Math.Max(0, Math.Ceiling(-Math.Log10(epsilon))));

                function = f;
                start = a;
                end = b;
                minimumX = x;
                minimumY = y;
                PrepareGraph();
                resultBox.Text =
                    $"x min ≈ {x.ToString("F" + decimals, CultureInfo.CurrentCulture)}\r\n" +
                    $"f(x)  ≈ {y.ToString("G12", CultureInfo.CurrentCulture)}\r\n\r\n" +
                    $"Итераций: {iterations}\r\n" +
                    $"[a; b] после поиска:\r\n" +
                    $"[{left:G10}; {right:G10}]\r\n\r\n" +
                    $"ε = {epsilon:G8}";
                plot.Invalidate();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException ||
                                       ex is ArithmeticException || ex is OverflowException)
            {
                function = null;
                resultBox.Text = "Ошибка: " + ex.Message;
                plot.Invalidate();
                MessageBox.Show(this, ex.Message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            if (values.Count == 0) { minGraphY = -1; maxGraphY = 1; return; }
            values.Sort();
            minGraphY = values[Math.Min(values.Count - 1, values.Count / 50)];
            maxGraphY = values[Math.Min(values.Count - 1, values.Count - 1 - values.Count / 50)];
            minGraphY = Math.Min(minGraphY, minimumY);
            maxGraphY = Math.Max(maxGraphY, minimumY);
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
            if (function == null)
            {
                TextRenderer.DrawText(g, "После расчёта здесь появится график",
                    Font, bounds, Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            Rectangle r = new Rectangle(67, 23, bounds.Width - 95, bounds.Height - 72);
            if (r.Width < 80 || r.Height < 80) return;
            double ySpan = maxGraphY - minGraphY;
            using var grid = new Pen(GridColor);
            using var axes = new Pen(Color.FromArgb(140, 151, 113));
            using var curve = new Pen(Accent, 2.0f);
            using var marker = new Pen(Marker, 1.3f) { DashStyle = DashStyle.Dash };
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
                float yy = r.Bottom - (float)((0 - minGraphY) / ySpan * r.Height);
                g.DrawLine(axes, r.Left, yy, r.Right, yy);
            }
            var previousClip = g.Save();
            try
            {
                g.SetClip(r);
                for (int i = 1; i <= Samples; i++)
                {
                    if (!graphValid[i - 1] || !graphValid[i]) continue;
                    double y0 = graphY[i - 1], y1 = graphY[i];
                    if ((y0 < minGraphY && y1 > maxGraphY) ||
                        (y1 < minGraphY && y0 > maxGraphY)) continue;
                    float py0 = r.Bottom - (float)((y0 - minGraphY) / ySpan * r.Height);
                    float py1 = r.Bottom - (float)((y1 - minGraphY) / ySpan * r.Height);
                    if (Math.Abs(py0) > 100000 || Math.Abs(py1) > 100000) continue;
                    float px0 = r.Left + r.Width * (i - 1) / (float)Samples;
                    float px1 = r.Left + r.Width * i / (float)Samples;
                    g.DrawLine(curve, px0, py0, px1, py1);
                }
                float mx = r.Left + (float)((minimumX - start) / (end - start) * r.Width);
                float my = r.Bottom - (float)((minimumY - minGraphY) / ySpan * r.Height);
                g.DrawLine(marker, mx, r.Top, mx, r.Bottom);
                using var dotBrush = new SolidBrush(Marker);
                g.FillEllipse(dotBrush, mx - 5, my - 5, 10, 10);
            }
            finally { g.Restore(previousClip); }
            using var labels = new Font("Consolas", 9f);
            using var brush = new SolidBrush(Foreground);
            g.DrawString(start.ToString("G5"), labels, brush, r.Left, r.Bottom + 6);
            string bLabel = end.ToString("G5");
            g.DrawString(bLabel, labels, brush, r.Right - g.MeasureString(bLabel, labels).Width, r.Bottom + 6);
            g.DrawString(maxGraphY.ToString("G4"), labels, brush, 2, r.Top);
            g.DrawString(minGraphY.ToString("G4"), labels, brush, 2, r.Bottom - 16);
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