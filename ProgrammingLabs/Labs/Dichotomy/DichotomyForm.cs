using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

// Лабораторная №1: поиск корня уравнения f(x) = 0 методом дихотомии.
// Оформление и вся логика находятся здесь; базовый дизайнер — в DichotomyForm.Designer.cs.
// Пространство имён формы: ProgrammingLabs.Labs.Dichotomy.
namespace ProgrammingLabs.Labs.Dichotomy
{
    // Класс разделён на тематические блоки (#region) для удобства чтения.
    public partial class DichotomyForm : Form
    {
        #region 1. Поля формы и состояние вычислений

        // Элементы окна: четыре поля ввода, ответ и панель графика.
        private readonly TextBox uiFormula = new TextBox();
        private readonly TextBox uiA = new TextBox();
        private readonly TextBox uiB = new TextBox();
        private readonly TextBox uiEpsilon = new TextBox();
        private readonly Label uiAnswer = new Label();
        private readonly GraphSurface uiGraph = new GraphSurface();

        // Последняя успешно вычисленная функция и данные для её графика.
        private ExpressionParser? currentFunction;
        private double currentA;
        private double currentB;
        private double currentRoot;
        private string currentExpressionText = string.Empty;

        #endregion

        #region 2. Создание и оформление интерфейса

        // Конструктор: сначала стандартный WinForms Designer, затем наша компоновка.
        public DichotomyForm()
        {
            InitializeComponent();
            BuildInterface();
        }

        // Компоновка: заголовок/меню -> поля ввода -> график и результат.
        private void BuildInterface()
        {
            Text = "Метод дихотомии — поиск корня f(x) = 0";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(860, 550);
            Size = new Size(1140, 710);
            BackColor = Color.FromArgb(238, 244, 242);
            Font = new Font("Segoe UI", 10F);
            Color ink = Color.FromArgb(32, 86, 91);
            Color muted = Color.FromArgb(108, 132, 135);

            // Верхняя панель: заголовок слева, действия в MenuStrip справа.
            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
            Panel headings = new Panel { Dock = DockStyle.Fill, Padding = new Padding(3, 3, 0, 0) };
            headings.Controls.Add(new Label
            {
                Text = "Поиск корня уравнения f(x) = 0",
                Dock = DockStyle.Top, Height = 39,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold), ForeColor = ink
            });
            headings.Controls.Add(new Label
            {
                Text = "Метод половинного деления · Лабораторная работа № 1",
                Dock = DockStyle.Bottom, Height = 26, ForeColor = muted
            });

            // Действия вынесены в MenuStrip согласно требованиям лабораторной.
            MenuStrip menu = new MenuStrip
            {
                Dock = DockStyle.Right, Width = 370, AutoSize = false,
                LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
                BackColor = Color.FromArgb(216, 235, 230),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(5, 13, 5, 13)
            };
            ToolStripMenuItem calculate = new ToolStripMenuItem("▶ Рассчитать")
            {
                ShortcutKeys = Keys.Control | Keys.Enter,
                ToolTipText = "Рассчитать корень (Ctrl+Enter)"
            };
            calculate.Click += (s, e) => CalculateRoot();
            ToolStripMenuItem clear = new ToolStripMenuItem("↻ Очистить");
            clear.Click += (s, e) => ClearInputs();
            // При закрытии модальной формы главное меню появляется снова.
            ToolStripMenuItem backToMenu = new ToolStripMenuItem("← В меню");
            backToMenu.Click += (s, e) => Close();
            menu.Items.AddRange(new ToolStripItem[] { calculate, clear, backToMenu });
            foreach (ToolStripItem item in menu.Items)
            {
                item.ForeColor = ink;
                item.Padding = new Padding(5, 6, 5, 6);
            }
            MainMenuStrip = menu;
            header.Controls.Add(headings);
            header.Controls.Add(menu);

            // Поля ввода расположены в одну строку над графиком.
            Panel inputsCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                Margin = new Padding(0, 3, 0, 13), Padding = new Padding(17, 13, 17, 7)
            };
            TableLayoutPanel inputGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2
            };
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19F));
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            inputGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            TextBox[] boxes = { uiFormula, uiA, uiB, uiEpsilon };
            string[] captions = { "Функция f(x)", "Граница a", "Граница b", "Точность ε" };
            for (int i = 0; i < boxes.Length; i++)
            {
                Panel cell = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 13, 0) };
                Label caption = new Label
                {
                    Text = captions[i], Dock = DockStyle.Top, Height = 27,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ink
                };
                SetupInput(boxes[i], i == 3 ? "0,0001" : "");
                cell.Controls.Add(boxes[i]);
                cell.Controls.Add(caption);
                inputGrid.Controls.Add(cell, i, 0);
            }
            // Подсказка по синтаксису вводимой формулы.
            Label hint = new Label
            {
                Text = "Примеры: x^3-x-2, sin(x)  ·  Функции: sin, cos, tan, sqrt, abs, ln, log, exp  ·  Степень: ^",
                Dock = DockStyle.Fill, ForeColor = muted, Font = new Font("Segoe UI", 9F)
            };
            inputGrid.Controls.Add(hint, 0, 1);
            inputGrid.SetColumnSpan(hint, 4);
            inputsCard.Controls.Add(inputGrid);

            // Основная область: широкий график слева, результат справа.
            TableLayoutPanel content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel graphCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                Padding = new Padding(15), Margin = new Padding(0, 0, 12, 0)
            };
            Label graphTitle = new Label
            {
                Text = "График функции", Dock = DockStyle.Top, Height = 39,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ink
            };
            uiGraph.Dock = DockStyle.Fill;
            uiGraph.BackColor = Color.White;
            uiGraph.Paint += DrawGraph;
            graphCard.Controls.Add(uiGraph);
            graphCard.Controls.Add(graphTitle);

            // Карточка с числовым ответом и легендой графика.
            Panel resultCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.FromArgb(216, 235, 230),
                Padding = new Padding(18), Margin = new Padding(0)
            };
            Label resultTitle = new Label
            {
                Text = "РЕЗУЛЬТАТ", Dock = DockStyle.Top, Height = 42,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ink
            };
            uiAnswer.Dock = DockStyle.Fill;
            uiAnswer.AutoSize = false;
            uiAnswer.Padding = new Padding(0, 12, 0, 0);
            uiAnswer.Font = new Font("Segoe UI", 11F);
            uiAnswer.ForeColor = Color.FromArgb(23, 108, 99);
            uiAnswer.Text = "Введите данные и нажмите «Рассчитать».";
            Label legend = new Label
            {
                Text = "Бирюзовая линия — функция.\nКоралловая точка — найденный корень.",
                Dock = DockStyle.Bottom, Height = 65, Font = new Font("Segoe UI", 9F),
                ForeColor = muted
            };
            resultCard.Controls.Add(uiAnswer);
            resultCard.Controls.Add(resultTitle);
            resultCard.Controls.Add(legend);

            content.Controls.Add(graphCard, 0, 0);
            content.Controls.Add(resultCard, 1, 0);
            // Итоговая сборка трёх горизонтальных уровней окна.
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1,
                Padding = new Padding(17), BackColor = BackColor
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 79F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 133F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(inputsCard, 0, 1);
            layout.Controls.Add(content, 0, 2);
            Controls.Add(layout);
            AcceptButton = null; // Действия — только в MenuStrip.
        }

        // Единое оформление текстовых полей.
        private static void SetupInput(TextBox box, string initialValue)
        {
            box.Dock = DockStyle.Top;
            box.Font = new Font("Segoe UI", 11F);
            box.BackColor = Color.FromArgb(247, 251, 250);
            box.ForeColor = Color.FromArgb(36, 70, 74);
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Text = initialValue;
            box.Margin = new Padding(0);
        }

        #endregion

        #region 3. Очистка и проверка ввода

        // Сброс данных, результата и графика.
        private void ClearInputs()
        {
            uiFormula.Clear();
            uiA.Clear();
            uiB.Clear();
            uiEpsilon.Clear();
            uiAnswer.Text = "Данные очищены.";
            currentFunction = null;
            currentExpressionText = string.Empty;
            uiGraph.Invalidate();
            uiFormula.Focus();
        }

        // Преобразование введённого числа: поддерживаются запятая и точка.
        private static double ReadNumber(string source, string parameter)
        {
            if (!double.TryParse(source.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double result) ||
                double.IsNaN(result) || double.IsInfinity(result))
            {
                throw new ArgumentException("Параметр «" + parameter +
                    "» должен быть вещественным числом.");
            }
            return result;
        }

        #endregion

        #region 4. Поиск корня методом дихотомии

        // Ищем x, для которого f(x) = 0, на заданном интервале [a, b].
        private void CalculateRoot()
        {
            try
            {
                string expression = uiFormula.Text.Trim();
                if (expression.Length == 0)
                    throw new ArgumentException("Введите формулу f(x).");

                // Разрешаем и обычный вид, и f(x) = ...
                if (expression.StartsWith("f(x)", StringComparison.OrdinalIgnoreCase))
                {
                    int equalsPosition = expression.IndexOf('=');
                    if (equalsPosition >= 0)
                        expression = expression.Substring(equalsPosition + 1).Trim();
                }

                // Считываем границы и проверяем точность.
                double a = ReadNumber(uiA.Text, "a");
                double b = ReadNumber(uiB.Text, "b");
                double eps = ReadNumber(uiEpsilon.Text, "ε");
                if (a >= b)
                    throw new ArgumentException("Должно выполняться a < b.");
                if (eps <= 0)
                    throw new ArgumentException("Точность ε должна быть больше нуля.");
                if (double.IsInfinity(b - a))
                    throw new ArgumentException("Слишком большой интервал [a, b].");

                // Вычисляем значения на концах интервала.
                ExpressionParser function = new ExpressionParser(expression);
                double fa = function.Calculate(a);
                double fb = function.Calculate(b);
                double root;
                int steps = 0;

                // Если один из концов уже является корнем, итерации не нужны.
                if (fa == 0)
                    root = a;
                else if (fb == 0)
                    root = b;
                else
                {
                    // Не перемножаем fa * fb: произведение может переполниться.
                    if ((fa > 0) == (fb > 0))
                        throw new ArgumentException(
                            "На концах интервала функция должна менять знак: " +
                            "f(a) и f(b) имеют одинаковый знак. Выберите другой интервал.");

                    // Сужаем отрезок вдвое, сохраняя смену знака на его концах.
                    double left = a, right = b;
                    root = left;
                    bool converged = false;
                    for (int i = 0; i < 2048; i++)
                    {
                        double mid = left + (right - left) / 2.0;
                        root = mid;
                        double fm = function.Calculate(mid);
                        steps++;
                        if (fm == 0 || (right - left) / 2.0 <= eps)
                        {
                            converged = true;
                            break;
                        }
                        if (mid == left || mid == right)
                        {
                            converged = true; // достигнут предел точности double
                            break;
                        }
                        if ((fa > 0) == (fm > 0))
                        {
                            left = mid;
                            fa = fm;
                        }
                        else
                        {
                            right = mid;
                        }
                    }
                    if (!converged)
                        throw new ArithmeticException("Не удалось достичь указанной точности.");
                }

                // Сохраняем найденное значение и обновляем результат на экране.
                double valueAtRoot = function.Calculate(root);
                currentFunction = function;
                currentExpressionText = expression;
                currentA = a;
                currentB = b;
                currentRoot = root;
                uiAnswer.Text =
                    "Корень x ≈ " + root.ToString("G16", CultureInfo.CurrentCulture) +
                    "\nЗначение f(x) ≈ " + valueAtRoot.ToString("G10", CultureInfo.CurrentCulture) +
                    "\nИтераций: " + steps.ToString(CultureInfo.CurrentCulture);
                uiGraph.Invalidate();
            }
            // Ошибки входных данных показываем пользователю, не завершая приложение.
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException ||
                                       ex is ArithmeticException || ex is OverflowException)
            {
                currentFunction = null;
                uiGraph.Invalidate();
                uiAnswer.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Некорректные данные",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 5. Построение графика функции

        // Paint-обработчик: координатная сетка, кривая f(x), отметка корня.
        private void DrawGraph(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = uiGraph.ClientRectangle;
            if (currentFunction == null)
            {
                TextRenderer.DrawText(g, "После расчёта здесь появится график f(x)",
                    Font, bounds, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Rectangle area = new Rectangle(65, 24, bounds.Width - 94, bounds.Height - 82);
            if (area.Width < 50 || area.Height < 50) return;

            const int count = 1600;
            double[] values = new double[count + 1];
            bool[] valid = new bool[count + 1];
            bool tangentLike = IsTangentLikeExpression(currentExpressionText);

            double minY = double.PositiveInfinity;
            double maxY = double.NegativeInfinity;
            double visibleLimit = tangentLike ? 4.0 : 1.0;
            int visibleCount = 0;

            for (int i = 0; i <= count; i++)
            {
                double x = currentA + (currentB - currentA) * (i / (double)count);
                try
                {
                    double y = currentFunction.Calculate(x);
                    if (double.IsNaN(y) || double.IsInfinity(y))
                        continue;

                    values[i] = y;
                    valid[i] = true;

                    if (!tangentLike)
                    {
                        double absY = Math.Abs(y);
                        if (absY < 1000)
                        {
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                            visibleCount++;
                        }
                    }
                }
                catch (FormatException) { }
                catch (ArithmeticException) { }
            }

            if (tangentLike)
            {
                minY = -visibleLimit;
                maxY = visibleLimit;
            }
            else
            {
                if (visibleCount == 0 || double.IsInfinity(minY) || double.IsInfinity(maxY))
                {
                    minY = -1;
                    maxY = 1;
                }
                if (Math.Abs(maxY - minY) < 1e-12)
                {
                    minY -= 1;
                    maxY += 1;
                }
                else
                {
                    double padding = (maxY - minY) * 0.1;
                    minY -= padding;
                    maxY += padding;
                }
                visibleLimit = Math.Max(Math.Abs(minY), Math.Abs(maxY));
            }

            double range = maxY - minY;

            using (Pen grid = new Pen(Color.FromArgb(228, 238, 233)))
            using (Pen axis = new Pen(Color.FromArgb(158, 175, 170), 1.3F))
            using (Pen functionPen = new Pen(Color.FromArgb(41, 148, 143), 2.2F))
            using (Pen rootPen = new Pen(Color.FromArgb(240, 128, 128), 1.6F))
            {
                for (int i = 0; i <= 5; i++)
                {
                    float px = area.Left + area.Width * i / 5F;
                    float py = area.Top + area.Height * i / 5F;
                    g.DrawLine(grid, px, area.Top, px, area.Bottom);
                    g.DrawLine(grid, area.Left, py, area.Right, py);
                }

                if (currentA <= 0 && currentB >= 0)
                {
                    float axisX = area.Left + (float)((0 - currentA) / (currentB - currentA) * area.Width);
                    g.DrawLine(axis, axisX, area.Top, axisX, area.Bottom);
                }
                if (minY <= 0 && maxY >= 0)
                {
                    float axisY = area.Bottom - (float)((0 - minY) / range * area.Height);
                    g.DrawLine(axis, area.Left, axisY, area.Right, axisY);
                }

                GraphicsState saved = g.Save();
                try
                {
                    g.SetClip(area);

                    if (tangentLike)
                        DrawTangentAsymptotes(g, grid, area, currentA, currentB);

                    for (int i = 1; i <= count; i++)
                    {
                        if (!valid[i - 1] || !valid[i])
                            continue;

                        double v1 = values[i - 1];
                        double v2 = values[i];
                        if (MustBreakSegment(v1, v2, visibleLimit, tangentLike))
                            continue;

                        double clipped1 = Math.Max(minY, Math.Min(maxY, v1));
                        double clipped2 = Math.Max(minY, Math.Min(maxY, v2));
                        double y1 = area.Bottom - (clipped1 - minY) / range * area.Height;
                        double y2 = area.Bottom - (clipped2 - minY) / range * area.Height;

                        if (double.IsNaN(y1) || double.IsNaN(y2) ||
                            double.IsInfinity(y1) || double.IsInfinity(y2))
                            continue;

                        float x1 = area.Left + area.Width * (i - 1) / (float)count;
                        float x2 = area.Left + area.Width * i / (float)count;
                        g.DrawLine(functionPen, x1, (float)y1, x2, (float)y2);
                    }

                    float rootX = area.Left + (float)((currentRoot - currentA) / (currentB - currentA) * area.Width);
                    rootPen.DashStyle = DashStyle.Dash;
                    g.DrawLine(rootPen, rootX, area.Top, rootX, area.Bottom);

                    double rootY = currentFunction.Calculate(currentRoot);
                    double clampedRootY = Math.Max(minY, Math.Min(maxY, rootY));
                    double rootPixelY = area.Bottom - (clampedRootY - minY) / range * area.Height;
                    if (!double.IsNaN(rootPixelY) && !double.IsInfinity(rootPixelY))
                    {
                        g.FillEllipse(Brushes.LightCoral, rootX - 5, (float)rootPixelY - 5, 10, 10);
                    }
                }
                finally
                {
                    g.Restore(saved);
                }
            }

            using (Font captionFont = new Font("Segoe UI", 9F))
            {
                g.DrawString(currentA.ToString("G5", CultureInfo.CurrentCulture), captionFont, Brushes.DimGray, area.Left, area.Bottom + 8);
                string bText = currentB.ToString("G5", CultureInfo.CurrentCulture);
                SizeF bTextSize = g.MeasureString(bText, captionFont);
                g.DrawString(bText, captionFont, Brushes.DimGray, area.Right - bTextSize.Width, area.Bottom + 8);
                g.DrawString(maxY.ToString("G4", CultureInfo.CurrentCulture), captionFont, Brushes.DimGray, 2, area.Top);
                g.DrawString(minY.ToString("G4", CultureInfo.CurrentCulture), captionFont, Brushes.DimGray, 2, area.Bottom - 17);
                g.DrawString("x ≈ " + currentRoot.ToString("G7", CultureInfo.CurrentCulture), captionFont, Brushes.LightCoral, area.Left + 8, area.Top + 6);
            }
        }

        private static bool IsTangentLikeExpression(string expression)
        {
            string text = expression.Replace(" ", string.Empty).ToLowerInvariant();
            return text.Contains("tg(") || text.Contains("tan(");
        }

        private static bool MustBreakSegment(double y1, double y2, double visibleLimit, bool tangentLike)
        {
            if (double.IsNaN(y1) || double.IsNaN(y2) || double.IsInfinity(y1) || double.IsInfinity(y2))
                return true;

            if (tangentLike)
            {
                if (Math.Abs(y1) > visibleLimit || Math.Abs(y2) > visibleLimit)
                    return true;
                if (Math.Abs(y2 - y1) > visibleLimit * 1.8)
                    return true;
            }
            else
            {
                if (Math.Abs(y1) > 1000 || Math.Abs(y2) > 1000)
                    return true;
            }

            return false;
        }

        private static void DrawTangentAsymptotes(Graphics g, Pen gridPen, Rectangle area, double a, double b)
        {
            using Pen asymptotePen = new Pen(Color.FromArgb(190, 190, 190), 1f) { DashStyle = DashStyle.Dash };
            double halfPi = Math.PI / 2.0;
            int startK = (int)Math.Ceiling((a - halfPi) / Math.PI);
            int endK = (int)Math.Floor((b - halfPi) / Math.PI);

            for (int k = startK; k <= endK; k++)
            {
                double x = halfPi + k * Math.PI;
                if (x <= a || x >= b) continue;
                float px = area.Left + (float)((x - a) / (b - a) * area.Width);
                g.DrawLine(asymptotePen, px, area.Top, px, area.Bottom);
            }
        }

        #endregion

        #region 6. Панель рисования графика

        // Двойная буферизация убирает мерцание при перерисовке графика.
        private sealed class GraphSurface : Panel
        {
            public GraphSurface()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        #endregion

        #region 7. Разбор и вычисление введённой формулы

        // Разбор пользовательской формулы без внешних библиотек.
        private sealed class ExpressionParser
        {
            private readonly string text;
            private int position;
            private double x;

            public ExpressionParser(string formula)
            {
                text = formula;
            }

            // Вычислить f(x): подставить x и полностью разобрать выражение.
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

            // Служебные операции для пропуска пробелов и проверки символов.
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

            // Приоритет 1: сложение и вычитание.
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

            // Приоритет 2: умножение и деление.
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

            // Унарные знаки + и -.
            private double ReadUnary()
            {
                if (Accept('+')) return ReadUnary();
                if (Accept('-')) return -ReadUnary();
                return ReadPower();
            }

            // Возведение в степень ^.
            private double ReadPower()
            {
                double value = ReadAtom();
                if (Accept('^')) value = Math.Pow(value, ReadUnary());
                return value;
            }

            // Скобки, числа, x, константы pi/e и функции sin/cos/...
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

            // Чтение числового литерала, включая дроби и экспоненциальную запись.
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

        #endregion
    }
}
