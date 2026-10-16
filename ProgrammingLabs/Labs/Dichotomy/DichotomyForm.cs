using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

 
 
 
namespace ProgrammingLabs.Labs.Dichotomy
{
     
    public partial class DichotomyForm : Form
    {

         
        private readonly TextBox uiFormula = new TextBox();
        private readonly TextBox uiA = new TextBox();
        private readonly TextBox uiB = new TextBox();
        private readonly TextBox uiEpsilon = new TextBox();
        private readonly TextBox uiAnswer = new TextBox();
        private readonly RadioButton uiOneRoot = new RadioButton();
        private readonly RadioButton uiAllRoots = new RadioButton();
        private readonly GraphSurface uiGraph = new GraphSurface();

         
        private ExpressionParser? currentFunction;
        private double currentA;
        private double currentB;
        private readonly List<double> currentRoots = new List<double>();
        private string currentExpressionText = string.Empty;



        private static readonly Color PageColor = Color.FromArgb(245, 248, 247);
        private static readonly Color DarkColor = Color.FromArgb(27, 65, 72);
        private static readonly Color AccentColor = Color.FromArgb(32, 138, 129);
        private static readonly Color MutedColor = Color.FromArgb(113, 137, 138);
        private static readonly Color BorderColor = Color.FromArgb(220, 233, 229);

         
        public DichotomyForm()
        {
            InitializeComponent();
            BuildInterface();
        }

        private void BuildInterface()
        {
            Text = "Метод дихотомии — поиск корня f(x) = 0";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(920, 645);
            Size = new Size(1220, 770);
            BackColor = PageColor;
            Font = new Font("Segoe UI", 10F);

             
            var header = new SoftCard
            {
                Dock = DockStyle.Fill,
                BackColor = DarkColor,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(25, 12, 18, 12)
            };
            var headerLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 405));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titles = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            titles.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            titles.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            titles.Controls.Add(MakeText("ПОИСК КОРНЯ УРАВНЕНИЯ  f(x) = 0", 16.5F, Color.White, true), 0, 0);
            titles.Controls.Add(MakeText("Лабораторная № 1   /   Метод половинного деления", 9.5F,
                Color.FromArgb(190, 220, 215)), 0, 1);

            var menu = new MenuStrip
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BackColor = DarkColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Padding(0, 13, 0, 13),
                Renderer = new MenuHighlightRenderer()
            };
            var calculate = new ToolStripMenuItem("▶ Рассчитать")
            {
                ShortcutKeys = Keys.Control | Keys.Enter,
                ToolTipText = "Рассчитать корень (Ctrl+Enter)"
            };
            calculate.Click += (s, e) => CalculateRoot();
            var clear = new ToolStripMenuItem("↻ Очистить");
            clear.Click += (s, e) => ClearInputs();
            var backToMenu = new ToolStripMenuItem("← В меню");
            backToMenu.Click += (s, e) => Close();
            menu.Items.AddRange(new ToolStripItem[] { calculate, clear, backToMenu });
            foreach (ToolStripItem item in menu.Items)
            {
                item.ForeColor = Color.White;
                item.Padding = new Padding(6, 7, 6, 7);
            }
            MainMenuStrip = menu;
            headerLayout.Controls.Add(titles, 0, 0);
            headerLayout.Controls.Add(menu, 1, 0);
            header.Controls.Add(headerLayout);

             
            var inputsCard = new SoftCard
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderColor = BorderColor,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(21, 14, 17, 8)
            };
            var inputs = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
            foreach (float width in new[] { 43F, 19F, 19F, 19F })
                inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TextBox[] fields = { uiFormula, uiA, uiB, uiEpsilon };
            string[] captions = { "ФУНКЦИЯ  f(x)", "ГРАНИЦА  a", "ГРАНИЦА  b", "ТОЧНОСТЬ  ε" };
            for (int i = 0; i < fields.Length; i++)
            {
                var caption = MakeText(captions[i], 9F, DarkColor, true);
                caption.Margin = new Padding(0, 0, 12, 0);
                inputs.Controls.Add(caption, i, 0);
                SetupInput(fields[i], i == 3 ? "0,0001" : "");
                var shell = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(244, 249, 247),
                    Padding = new Padding(11, 10, 11, 9),
                    Margin = new Padding(0, 0, 14, 2)
                };
                shell.Paint += (s, e) =>
                {
                    using var border = new Pen(BorderColor);
                    e.Graphics.DrawRectangle(border, 0, 0, shell.Width - 1, shell.Height - 1);
                };
                shell.Controls.Add(fields[i]);
                inputs.Controls.Add(shell, i, 1);
            }
            var syntax = MakeText("Примеры: x^3-x-2, tg(x), sin(x)   •   Функции: sin, cos, tan, sqrt, abs, ln, log, exp   •   Степень: ^",
                9F, MutedColor);
            syntax.Margin = new Padding(0, 5, 0, 0);
            inputs.Controls.Add(syntax, 0, 2);
            inputs.SetColumnSpan(syntax, 4);

             
            var modeRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            var modeTitle = MakeText("РЕЖИМ ПОИСКА", 9F, DarkColor, true);
            modeTitle.Dock = DockStyle.None;
            modeTitle.Size = new Size(145, 33);
            modeRow.Controls.Add(modeTitle);
            SetupModeButton(uiOneRoot, "Один корень");
            SetupModeButton(uiAllRoots, "Все корни");
            modeRow.Controls.Add(uiOneRoot);
            modeRow.Controls.Add(uiAllRoots);
            inputs.Controls.Add(modeRow, 0, 3);
            inputs.SetColumnSpan(modeRow, 4);
            uiOneRoot.Checked = true;
            uiOneRoot.CheckedChanged += (s, e) => UpdateMode();
            uiAllRoots.CheckedChanged += (s, e) => UpdateMode();
            UpdateModeColors();
            inputsCard.Controls.Add(inputs);

             
            var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var graphCard = new SoftCard
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderColor = BorderColor,
                Padding = new Padding(19, 14, 19, 15),
                Margin = new Padding(0, 0, 14, 0)
            };
            var graphTitle = MakeText("График функции", 13F, DarkColor, true);
            graphTitle.Dock = DockStyle.Top;
            graphTitle.Height = 43;
            var graphFooter = MakeText("Бирюзовая — f(x)     •     Коралловые точки — найденные корни", 9F, MutedColor);
            graphFooter.Dock = DockStyle.Bottom;
            graphFooter.Height = 32;
            uiGraph.Dock = DockStyle.Fill;
            uiGraph.BackColor = Color.White;
            uiGraph.Paint += DrawGraph;
            graphCard.Controls.Add(uiGraph);
            graphCard.Controls.Add(graphFooter);
            graphCard.Controls.Add(graphTitle);

            var resultCard = new SoftCard
            {
                Dock = DockStyle.Fill,
                BackColor = DarkColor,
                Padding = new Padding(24, 21, 22, 21),
                Margin = new Padding(0)
            };
            var resultTitle = MakeText("РЕЗУЛЬТАТ РАСЧЁТА", 10F,
                Color.FromArgb(153, 220, 206), true);
            resultTitle.Dock = DockStyle.Top;
            resultTitle.Height = 43;
            uiAnswer.Dock = DockStyle.Fill;
            uiAnswer.Multiline = true;
            uiAnswer.ReadOnly = true;
            uiAnswer.ScrollBars = ScrollBars.Vertical;
            uiAnswer.BorderStyle = BorderStyle.None;
            uiAnswer.WordWrap = false;
            uiAnswer.ForeColor = Color.White;
            uiAnswer.BackColor = DarkColor;
            uiAnswer.Font = new Font("Segoe UI", 11F);
            uiAnswer.Padding = new Padding(0, 14, 0, 0);
            uiAnswer.Text = "Введите функцию и границы интервала, затем нажмите «Рассчитать».";
            var resultFooter = MakeText("Метод дихотомии\nТочность задаётся по оси x", 9.5F,
                Color.FromArgb(185, 212, 211));
            resultFooter.Dock = DockStyle.Bottom;
            resultFooter.Height = 75;
            resultCard.Controls.Add(uiAnswer);
            resultCard.Controls.Add(resultTitle);
            resultCard.Controls.Add(resultFooter);

            workspace.Controls.Add(graphCard, 0, 0);
            workspace.Controls.Add(resultCard, 1, 0);

             
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(22, 21, 22, 22),
                BackColor = PageColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 194));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(inputsCard, 0, 1);
            root.Controls.Add(workspace, 0, 2);
            Controls.Add(root);
            AcceptButton = null;  
        }

        private static Label MakeText(string content, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = content,
                Dock = DockStyle.Fill,
                ForeColor = color,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private static void SetupInput(TextBox box, string initialValue)
        {
            box.Dock = DockStyle.Fill;
            box.AutoSize = false;
            box.Font = new Font("Segoe UI", 11F);
            box.BackColor = Color.FromArgb(244, 249, 247);
            box.ForeColor = DarkColor;
            box.BorderStyle = BorderStyle.None;
            box.Text = initialValue;
            box.Margin = Padding.Empty;
        }

         
        private static void SetupModeButton(RadioButton button, string caption)
        {
            button.Text = caption;
            button.Appearance = Appearance.Button;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Size = new Size(140, 32);
            button.Margin = new Padding(0, 0, 8, 0);
            button.UseVisualStyleBackColor = false;
        }

        private void UpdateModeColors()
        {
            foreach (RadioButton button in new[] { uiOneRoot, uiAllRoots })
            {
                button.BackColor = button.Checked ? AccentColor : Color.FromArgb(237, 246, 243);
                button.ForeColor = button.Checked ? Color.White : DarkColor;
                button.FlatAppearance.BorderColor = button.Checked ? AccentColor : BorderColor;
            }
        }

        private void UpdateMode()
        {
            UpdateModeColors();
            currentFunction = null;
            currentRoots.Clear();
            uiAnswer.Text = "Выберите функцию, интервал и нажмите «Рассчитать».";
            uiGraph.Invalidate();
        }

         
        private sealed class MenuHighlightRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Color background = e.Item.Selected
                    ? Color.FromArgb(43, 114, 114)
                    : e.Item.Text?.Contains("Рассчитать") == true ? AccentColor : DarkColor;
                using var brush = new SolidBrush(background);
                e.Graphics.FillRectangle(brush, 2, 2, Math.Max(0, e.Item.Width - 4),
                    Math.Max(0, e.Item.Height - 4));
            }
        }

         
        private sealed class SoftCard : Panel
        {
             
            public Color BorderColor = Color.Transparent;

            public SoftCard()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            private static GraphicsPath Rounded(Rectangle r, int radius)
            {
                float d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
                var path = new GraphicsPath();
                path.AddArc(r.Left, r.Top, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (Width < 2 || Height < 2) return;
                using var path = Rounded(new Rectangle(0, 0, Width, Height), 14);
                var previous = Region;
                Region = new Region(path);
                previous?.Dispose();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (BorderColor == Color.Transparent || Width < 3 || Height < 3) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = Rounded(new Rectangle(1, 1, Width - 2, Height - 2), 13);
                using var border = new Pen(BorderColor);
                e.Graphics.DrawPath(border, path);
            }
        }



         
        private void ClearInputs()
        {
            uiFormula.Clear();
            uiA.Clear();
            uiB.Clear();
            uiEpsilon.Clear();
            uiAnswer.Text = "Данные очищены.";
            currentFunction = null;
            currentRoots.Clear();
            currentExpressionText = string.Empty;
            uiGraph.Invalidate();
            uiFormula.Focus();
        }

         
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



         
        private void CalculateRoot()
        {
            try
            {
                string expression = uiFormula.Text.Trim();
                if (expression.Length == 0)
                    throw new ArgumentException("Введите формулу f(x).");

                 
                if (expression.StartsWith("f(x)", StringComparison.OrdinalIgnoreCase))
                {
                    int equalsPosition = expression.IndexOf('=');
                    if (equalsPosition >= 0)
                        expression = expression.Substring(equalsPosition + 1).Trim();
                }

                 
                double a = ReadNumber(uiA.Text, "a");
                double b = ReadNumber(uiB.Text, "b");
                double eps = ReadNumber(uiEpsilon.Text, "ε");
                if (a >= b)
                    throw new ArgumentException("Должно выполняться a < b.");
                if (eps <= 0)
                    throw new ArgumentException("Точность ε должна быть больше нуля.");
                if (double.IsInfinity(b - a))
                    throw new ArgumentException("Слишком большой интервал [a, b].");

                ExpressionParser function = new ExpressionParser(expression);

                 
                 
                 
                double fa = function.Calculate(a);
                double fb = function.Calculate(b);
                if ((fa > 0 && fb > 0) || (fa < 0 && fb < 0))
                    throw new ArgumentException(
                        "Для метода дихотомии f(a) и f(b) должны иметь разные знаки. " +
                        "Сейчас оба значения положительны или оба отрицательны. " +
                        "Выберите другой интервал.");

                if (uiAllRoots.Checked)
                {
                     
                    List<double> roots = FindAllRoots(function, a, b, eps, out int refinementSteps);
                    currentFunction = function;
                    currentExpressionText = expression;
                    currentA = a;
                    currentB = b;
                    currentRoots.Clear();
                    currentRoots.AddRange(roots);

                    var report = new StringBuilder();
                    report.AppendLine("Найдено корней: " + roots.Count);
                    report.AppendLine();
                    for (int i = 0; i < roots.Count; i++)
                    {
                        double rootValue = function.Calculate(roots[i]);
                        report.AppendLine((i + 1) + ".  x ≈ " +
                            FormatRoot(roots[i], eps));
                        report.AppendLine("     f(x) ≈ " +
                            rootValue.ToString("G6", CultureInfo.CurrentCulture));
                        report.AppendLine();
                    }
                    if (roots.Count == 0)
                        report.AppendLine("Корней на выбранном интервале не обнаружено.");
                    report.AppendLine("Шагов уточнения: " + refinementSteps);
                    uiAnswer.Text = report.ToString();
                    uiGraph.Invalidate();
                    return;
                }

                 
                 
                double root;
                int steps = 0;
                if (fa == 0)
                    root = a;
                else if (fb == 0)
                    root = b;
                else
                {
                    double left = a, right = b;
                    root = left;
                    bool converged = false;
                    for (int i = 0; i < 2048; i++)
                    {
                        double mid = left + (right - left) / 2.0;
                        root = mid;
                        double fm = function.Calculate(mid);
                        steps++;
                         
                        if (fm == 0 || (right - left) / 2.0 <= eps / 2.0)
                        {
                            converged = true;
                            break;
                        }
                        if (mid == left || mid == right)
                        {
                            converged = true;
                            break;
                        }
                        if ((fa > 0) == (fm > 0))
                        {
                            left = mid;
                            fa = fm;
                        }
                        else
                            right = mid;
                    }
                    if (!converged)
                        throw new ArithmeticException("Не удалось достичь указанной точности.");
                }

                double valueAtRoot = function.Calculate(root);
                 
                 
                if (Math.Abs(valueAtRoot) >
                    Math.Max(1.0, Math.Max(Math.Abs(function.Calculate(a)),
                                               Math.Abs(function.Calculate(b)))) * 10)
                    throw new ArithmeticException("Найден разрыв функции, а не её корень. " +
                        "Выберите другой интервал или режим «Все корни».");

                currentFunction = function;
                currentExpressionText = expression;
                currentA = a;
                currentB = b;
                currentRoots.Clear();
                currentRoots.Add(root);
                uiAnswer.Text =
                    "Корень x ≈ " + FormatRoot(root, eps) +
                    Environment.NewLine + "Значение f(x) ≈ " +
                    valueAtRoot.ToString("G10", CultureInfo.CurrentCulture) +
                    Environment.NewLine + "Итераций: " +
                    steps.ToString(CultureInfo.CurrentCulture);
                uiGraph.Invalidate();
            }
             
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException ||
                                       ex is ArithmeticException || ex is OverflowException)
            {
                currentFunction = null;
                currentRoots.Clear();
                uiGraph.Invalidate();
                uiAnswer.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Некорректные данные",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

         
         
         
         
        private static List<double> FindAllRoots(ExpressionParser function,
            double a, double b, double eps, out int steps)
        {
            const int samples = 4000;
            var roots = new List<double>();
            steps = 0;
            double xPrevious = a, yPrevious = 0;
            double xBeforePrevious = a, yBeforePrevious = 0;
            bool previousValid = TryEvaluate(function, a, out yPrevious);
            bool beforePreviousValid = false;
            int validSamples = previousValid ? 1 : 0;
            int consecutiveExactZeros = previousValid && yPrevious == 0 ? 1 : 0;
            double duplicateRadius = Math.Max(eps * 2, (b - a) / samples * 0.5);

            if (previousValid && Math.Abs(yPrevious) < 1e-12)
                AddUniqueRoot(roots, a, duplicateRadius);

            for (int i = 1; i <= samples; i++)
            {
                double x = a + (b - a) * i / samples;
                bool valid = TryEvaluate(function, x, out double y);
                if (valid) validSamples++;
                consecutiveExactZeros = valid && y == 0 ? consecutiveExactZeros + 1 : 0;
                if (consecutiveExactZeros >= 3)
                    throw new ArgumentException("Функция равна нулю на целом участке: " +
                        "корней бесконечно много, перечислить их нельзя.");

                if (valid && (y == 0 || (i == samples && Math.Abs(y) < 1e-12)))
                    AddUniqueRoot(roots, x, duplicateRadius);

                if (previousValid && valid && ((yPrevious < 0 && y > 0) ||
                                               (yPrevious > 0 && y < 0)))
                {
                    if (TryBisect(function, xPrevious, x, yPrevious, y,
                        eps, ref steps, out double root))
                        AddUniqueRoot(roots, root, duplicateRadius);
                }

                 
                if (beforePreviousValid && previousValid && valid &&
                    Math.Abs(yPrevious) < Math.Abs(yBeforePrevious) &&
                    Math.Abs(yPrevious) < Math.Abs(y))
                {
                    if (TryFindTouchingRoot(function, xBeforePrevious, x,
                        eps, ref steps, out double touchingRoot))
                        AddUniqueRoot(roots, touchingRoot, duplicateRadius);
                }

                xBeforePrevious = xPrevious;
                yBeforePrevious = yPrevious;
                beforePreviousValid = previousValid;
                xPrevious = x;
                yPrevious = y;
                previousValid = valid;

                if (roots.Count > 500)
                    throw new ArgumentException("Найдено слишком много корней. " +
                        "Сузьте интервал поиска.");
            }

            if (validSamples == 0)
                throw new ArgumentException("Не удалось вычислить функцию на интервале. " +
                    "Проверьте формулу и область определения.");
            roots.Sort();
            return roots;
        }

        private static bool TryEvaluate(ExpressionParser function, double x, out double y)
        {
            try
            {
                y = function.Calculate(x);
                return !double.IsNaN(y) && !double.IsInfinity(y);
            }
            catch (ArithmeticException) { }
            catch (FormatException) { }
            y = 0;
            return false;
        }

         
         
         
        private static bool TryBisect(ExpressionParser function,
            double left, double right, double leftValue, double rightValue,
            double eps, ref int steps, out double root)
        {
            double initialMagnitude = Math.Min(Math.Abs(leftValue), Math.Abs(rightValue));
            root = left + (right - left) / 2.0;
            bool accuracyReached = false;

            for (int i = 0; i < 80; i++)
            {
                double mid = left + (right - left) / 2.0;
                if (mid == left || mid == right) break;
                if (!TryEvaluate(function, mid, out double fm)) return false;

                 
                 
                if (!accuracyReached)
                {
                    root = mid;
                    steps++;
                    if (fm == 0 || (right - left) / 2.0 <= eps / 2.0)
                        accuracyReached = true;
                }

                if (fm == 0) return true;
                if ((leftValue > 0) == (fm > 0))
                {
                    left = mid;
                    leftValue = fm;
                }
                else
                {
                    right = mid;
                    rightValue = fm;
                }
                if (right - left <= 1e-13 * Math.Max(1.0, Math.Abs(mid)))
                    break;
            }

             
             
            double checkX = left + (right - left) / 2.0;
            return TryEvaluate(function, checkX, out double residual) &&
                Math.Abs(residual) <= Math.Max(1e-9, initialMagnitude * 1e-6);
        }

         
         
        private static bool TryFindTouchingRoot(ExpressionParser function,
            double left, double right, double eps, ref int steps, out double root)
        {
            const double ratio = 0.6180339887498949;
            if (!TryEvaluate(function, left, out double fLeft) ||
                !TryEvaluate(function, right, out double fRight))
            {
                root = left;
                return false;
            }

            double edgeMagnitude = Math.Min(Math.Abs(fLeft), Math.Abs(fRight));
            double p = right - (right - left) * ratio;
            double q = left + (right - left) * ratio;
            if (!TryEvaluate(function, p, out double fp) ||
                !TryEvaluate(function, q, out double fq))
            {
                root = left;
                return false;
            }

            for (int i = 0; i < 65 && right - left > eps / 2.0; i++)
            {
                steps++;
                if (Math.Abs(fp) < Math.Abs(fq))
                {
                    right = q;
                    q = p;
                    fq = fp;
                    p = right - (right - left) * ratio;
                    if (!TryEvaluate(function, p, out fp)) break;
                }
                else
                {
                    left = p;
                    p = q;
                    fp = fq;
                    q = left + (right - left) * ratio;
                    if (!TryEvaluate(function, q, out fq)) break;
                }
                if (right - left <= 1e-14 * Math.Max(1.0, Math.Abs(p)))
                    break;
            }

            root = Math.Abs(fp) < Math.Abs(fq) ? p : q;
             
            return TryEvaluate(function, root, out double residual) &&
                Math.Abs(residual) <= Math.Max(1e-12, edgeMagnitude * 0.4);
        }

         
         
        private static string FormatRoot(double root, double eps)
        {
            int digits = Math.Max(0, Math.Min(15,
                (int)Math.Ceiling(-Math.Log10(eps) - 1e-12)));
            if (Math.Abs(root) < Math.Pow(10, -digits) / 2.0)
                root = 0;
            return root.ToString("F" + digits, CultureInfo.CurrentCulture);
        }

         
        private static void AddUniqueRoot(List<double> roots, double root, double tolerance)
        {
            foreach (double existing in roots)
                if (Math.Abs(existing - root) <= tolerance) return;
            roots.Add(root);
        }



         
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

            using (Pen grid = new Pen(Color.FromArgb(232, 240, 236)))
            using (Pen axis = new Pen(Color.FromArgb(162, 180, 178), 1.3F))
            using (Pen functionPen = new Pen(AccentColor, 2.4F))
            using (Pen rootPen = new Pen(Color.FromArgb(234, 119, 115), 1.6F))
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

                     
                     
                    foreach (double root in currentRoots)
                    {
                        float rootX = area.Left + (float)((root - currentA) /
                            (currentB - currentA) * area.Width);
                        if (currentRoots.Count == 1)
                        {
                            rootPen.DashStyle = DashStyle.Dash;
                            g.DrawLine(rootPen, rootX, area.Top, rootX, area.Bottom);
                        }
                        if (!TryEvaluate(currentFunction, root, out double rootY))
                            continue;
                        double clampedRootY = Math.Max(minY, Math.Min(maxY, rootY));
                        double rootPixelY = area.Bottom - (clampedRootY - minY) / range * area.Height;
                        if (!double.IsNaN(rootPixelY) && !double.IsInfinity(rootPixelY))
                            g.FillEllipse(Brushes.Salmon, rootX - 5, (float)rootPixelY - 5, 10, 10);
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
                string rootCaption = currentRoots.Count == 1
                    ? "x ≈ " + currentRoots[0].ToString("G7", CultureInfo.CurrentCulture)
                    : "Найдено корней: " + currentRoots.Count;
                g.DrawString(rootCaption, captionFont, Brushes.Salmon, area.Left + 8, area.Top + 6);
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



         
        private sealed class GraphSurface : Panel
        {
            public GraphSurface()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
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