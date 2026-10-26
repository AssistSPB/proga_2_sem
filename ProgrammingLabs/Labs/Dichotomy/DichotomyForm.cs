using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace ProgrammingLabs.Labs.Dichotomy {
    public partial class DichotomyForm : Form {

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
        private const int GraphSamples = 1600;
        private readonly double[] graphValues = new double[GraphSamples + 1];
        private readonly bool[] graphValid = new bool[GraphSamples + 1];
        private double graphMinY = -1, graphMaxY = 1, graphVisibleLimit = 1;
        private bool graphTangentLike;


        private static readonly Color PageColor = Color.FromArgb(49, 53, 39);
        private static readonly Color DarkColor = Color.FromArgb(51, 56, 41);
        private static readonly Color AccentColor = Color.FromArgb(180, 190, 143);
        private static readonly Color MutedColor = Color.FromArgb(169, 180, 151);
        private static readonly Color BorderColor = Color.FromArgb(104, 112, 86);
        private static readonly Color SurfaceColor = Color.FromArgb(61, 66, 50);

        public DichotomyForm()
        {
            InitializeComponent();
            BuildInterface();
        }

        private void BuildInterface()
        {
            SuspendLayout();
            Text = "Dichotomy";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(860, 560);
            Size = new Size(1120, 690);
            BackColor = PageColor;
            Font = new Font("Tahoma", 9F);

            var root = new TableLayoutPanel {
                Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1,
                Padding = new Padding(10), BackColor = PageColor
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 121));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel {
                Dock = DockStyle.Fill, ColumnCount = 2, BackColor = SurfaceColor,
                Margin = new Padding(0, 0, 0, 6)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 335));
            header.Controls.Add(MakeText("  Console  /  Dichotomy: f(x) = 0", 12, AccentColor, true), 0, 0);
            var menu = new MenuStrip {
                Dock = DockStyle.Fill, BackColor = SurfaceColor, AutoSize = false,
                Font = new Font("Tahoma", 9F), Padding = new Padding(0, 6, 0, 5)
            };
            var run = new ToolStripMenuItem("Рассчитать") { ForeColor = AccentColor, ShortcutKeys = Keys.Control | Keys.Enter, ShowShortcutKeys = false };
            var clear = new ToolStripMenuItem("Очистить") { ForeColor = AccentColor };
            var back = new ToolStripMenuItem("В меню") { ForeColor = AccentColor };
            run.Click += (_, _) => CalculateRoot();
            clear.Click += (_, _) => ClearInputs();
            back.Click += (_, _) => Close();
            menu.Items.AddRange(new ToolStripItem[] { run, clear, back });
            MainMenuStrip = menu;
            header.Controls.Add(menu, 1, 0);
            root.Controls.Add(header, 0, 0);

            var inputs = new TableLayoutPanel {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(12, 8, 12, 6),
                BackColor = SurfaceColor, Margin = new Padding(0, 0, 0, 7)
            };
            foreach (float p in new[] { 42F, 19F, 19F, 20F })
                inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, p));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var boxes = new[] { uiFormula, uiA, uiB, uiEpsilon };
            var names = new[] { "f(x)", "a", "b", "ε" };
            for (int i = 0; i < boxes.Length; i++)
            {
                inputs.Controls.Add(MakeText(names[i], 9, AccentColor), i, 0);
                SetupInput(boxes[i], i == 3 ? "0,0001" : "");
                boxes[i].Margin = new Padding(0, 0, 8, 0);
                inputs.Controls.Add(boxes[i], i, 1);
            }
            var modes = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
            SetupModeButton(uiOneRoot, "Один корень");
            SetupModeButton(uiAllRoots, "Все корни");
            modes.Controls.Add(uiOneRoot);
            modes.Controls.Add(uiAllRoots);
            inputs.Controls.Add(modes, 0, 2);
            inputs.SetColumnSpan(modes, 4);
            uiOneRoot.Checked = true;
            uiOneRoot.CheckedChanged += (_, _) => UpdateMode();
            uiAllRoots.CheckedChanged += (_, _) => UpdateMode();
            UpdateModeColors();
            root.Controls.Add(inputs, 0, 1);

            var area = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            var graph = new Panel { Dock = DockStyle.Fill, BackColor = DarkColor, Padding = new Padding(10), Margin = new Padding(0, 0, 8, 0) };
            var graphTitle = MakeText("График", 10, AccentColor, true);
            graphTitle.Dock = DockStyle.Top; graphTitle.Height = 27;
            uiGraph.Dock = DockStyle.Fill;
            uiGraph.BackColor = DarkColor;
            uiGraph.Paint += DrawGraph;
            graph.Controls.Add(uiGraph);
            graph.Controls.Add(graphTitle);

            var result = new Panel { Dock = DockStyle.Fill, BackColor = SurfaceColor, Padding = new Padding(10), Margin = Padding.Empty };
            var title = MakeText("Результат", 10, AccentColor, true);
            title.Dock = DockStyle.Top; title.Height = 28;
            uiAnswer.Dock = DockStyle.Fill;
            uiAnswer.Multiline = true;
            uiAnswer.ReadOnly = true;
            uiAnswer.ScrollBars = ScrollBars.Vertical;
            uiAnswer.BorderStyle = BorderStyle.None;
            uiAnswer.BackColor = SurfaceColor;
            uiAnswer.ForeColor = Color.FromArgb(220, 222, 199);
            uiAnswer.Font = new Font("Consolas", 10F);
            uiAnswer.Text = "Нет расчёта";
            result.Controls.Add(uiAnswer);
            result.Controls.Add(title);
            area.Controls.Add(graph, 0, 0);
            area.Controls.Add(result, 1, 0);
            root.Controls.Add(area, 0, 2);
            Controls.Add(root);
            ResumeLayout(false);
        }

        private static Label MakeText(string content, float size, Color color, bool bold = false)
        {
            return new Label {
                Text = content, Dock = DockStyle.Fill, ForeColor = color,
                Font = new Font("Tahoma", size, bold ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
            };
        }

        private static void SetupInput(TextBox box, string initialValue)
        {
            box.Dock = DockStyle.Fill;
            box.BackColor = Color.FromArgb(46, 51, 39);
            box.ForeColor = Color.FromArgb(224, 226, 206);
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = new Font("Consolas", 11F);
            box.Text = initialValue;
        }

        private static void SetupModeButton(RadioButton button, string caption)
        {
            button.Text = caption;
            button.Appearance = Appearance.Button;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Tahoma", 9F);
            button.Size = new Size(122, 26);
            button.Margin = new Padding(0, 0, 7, 0);
            button.UseVisualStyleBackColor = false;
        }

        private void UpdateModeColors()
        {
            foreach (RadioButton button in new[] { uiOneRoot, uiAllRoots })
            {
                button.BackColor = button.Checked ? AccentColor : DarkColor;
                button.ForeColor = button.Checked ? DarkColor : MutedColor;
                button.FlatAppearance.BorderColor = button.Checked ? AccentColor : BorderColor;
            }
        }

        private void UpdateMode()
        {
            UpdateModeColors();
            currentFunction = null;
            currentRoots.Clear();
            uiAnswer.Text = "Нет расчёта";
            uiGraph.Invalidate();
        }

        private void ClearInputs()
        {
            uiFormula.Clear();
            uiA.Clear();
            uiB.Clear();
            uiEpsilon.Clear();
            uiAnswer.Text = "Очищено";
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
                    CacheGraphSamples();
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
                CacheGraphSamples();
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


        private void CacheGraphSamples()
        {
            if (currentFunction == null) return;
            bool tangentLike = IsTangentLikeExpression(currentExpressionText);
            double minY = double.PositiveInfinity;
            double maxY = double.NegativeInfinity;
            double visibleLimit = tangentLike ? 4.0 : 1.0;
            int visibleCount = 0;

            Array.Clear(graphValid, 0, graphValid.Length);
            for (int i = 0; i <= GraphSamples; i++)
            {
                double x = currentA + (currentB - currentA) * (i / (double)GraphSamples);
                if (!TryEvaluate(currentFunction, x, out double y)) continue;
                graphValues[i] = y;
                graphValid[i] = true;
                if (!tangentLike && Math.Abs(y) < 1000)
                {
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                    visibleCount++;
                }
            }
            if (tangentLike) { minY = -4; maxY = 4; }
            else if (visibleCount == 0 || double.IsInfinity(minY) || double.IsInfinity(maxY))
            { minY = -1; maxY = 1; }
            else if (Math.Abs(maxY - minY) < 1e-12) { minY -= 1; maxY += 1; }
            else
            {
                double padding = (maxY - minY) * 0.1;
                minY -= padding;
                maxY += padding;
            }
            if (!tangentLike) visibleLimit = Math.Max(Math.Abs(minY), Math.Abs(maxY));
            graphMinY = minY;
            graphMaxY = maxY;
            graphVisibleLimit = visibleLimit;
            graphTangentLike = tangentLike;
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

            const int count = GraphSamples;
            double[] values = graphValues;
            bool[] valid = graphValid;
            bool tangentLike = graphTangentLike;
            double minY = graphMinY, maxY = graphMaxY;
            double visibleLimit = graphVisibleLimit;

            double range = maxY - minY;

            using (Pen grid = new Pen(Color.FromArgb(43, 60, 42)))
            using (Pen axis = new Pen(Color.FromArgb(120, 140, 108), 1.3F))
            using (Pen functionPen = new Pen(AccentColor, 2.4F))
            using (Pen rootPen = new Pen(Color.FromArgb(255, 167, 68), 1.6F))
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
                            g.FillEllipse(Brushes.Orange, rootX - 5, (float)rootPixelY - 5, 10, 10);
                    }
                }
                finally
                {
                    g.Restore(saved);
                }
            }

            using (Font captionFont = new Font("Consolas", 9F))
            {
                g.DrawString(currentA.ToString("G5", CultureInfo.CurrentCulture), captionFont, Brushes.LightGreen, area.Left, area.Bottom + 8);
                string bText = currentB.ToString("G5", CultureInfo.CurrentCulture);
                SizeF bTextSize = g.MeasureString(bText, captionFont);
                g.DrawString(bText, captionFont, Brushes.LightGreen, area.Right - bTextSize.Width, area.Bottom + 8);
                g.DrawString(maxY.ToString("G4", CultureInfo.CurrentCulture), captionFont, Brushes.LightGreen, 2, area.Top);
                g.DrawString(minY.ToString("G4", CultureInfo.CurrentCulture), captionFont, Brushes.LightGreen, 2, area.Bottom - 17);
                string rootCaption = currentRoots.Count == 1
                    ? "x ≈ " + currentRoots[0].ToString("G7", CultureInfo.CurrentCulture)
                    : "Найдено корней: " + currentRoots.Count;
                g.DrawString(rootCaption, captionFont, Brushes.Orange, area.Left + 8, area.Top + 6);
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
            using Pen asymptotePen = new Pen(Color.FromArgb(84, 106, 78), 1f) { DashStyle = DashStyle.Dash };
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