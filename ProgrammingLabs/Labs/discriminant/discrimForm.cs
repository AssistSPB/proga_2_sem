using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ProgrammingLabs.Labs.discriminant {
    public partial class discrimForm : Form {
        private static readonly Color Background = Color.FromArgb(49, 53, 39);
        private static readonly Color Surface = Color.FromArgb(60, 65, 48);
        private static readonly Color Foreground = Color.FromArgb(215, 215, 190);
        private static readonly Color Accent = Color.FromArgb(172, 185, 132);

        private readonly TextBox aBox = new TextBox();
        private readonly TextBox bBox = new TextBox();
        private readonly TextBox cBox = new TextBox();
        private readonly TextBox answerBox = new TextBox();

        public discrimForm() {
            InitializeComponent();
            BuildInterface();
        }

        private static Label MakeLabel(string text) => new Label {
            Text = text,
            Dock = DockStyle.Fill,
            ForeColor = Accent,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Tahoma", 10F)
        };

        private void BuildInterface() {
            Text = "Discriminant / Vieta";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(570, 400);
            Size = new Size(670, 480);
            BackColor = Background;
            Font = new Font("Tahoma", 10F);

            var layout = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 115));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Surface };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
            var heading = MakeLabel("  Console / Discriminant & Vieta");
            heading.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            header.Controls.Add(heading, 0, 0);

            var menu = new MenuStrip { Dock = DockStyle.Fill, AutoSize = false, BackColor = Surface };
            var calculate = new ToolStripMenuItem("Рассчитать") { ForeColor = Accent, ShortcutKeys = Keys.Control | Keys.Enter, ShowShortcutKeys = false };
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
                Dock = DockStyle.Fill, BackColor = Surface,
                Padding = new Padding(12, 8, 12, 8), ColumnCount = 3, RowCount = 3,
                Margin = new Padding(0, 8, 0, 8)
            };
            for (int i = 0; i < 3; i++) inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
            inputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var boxes = new[] { aBox, bBox, cBox };
            var names = new[] { "a", "b", "c" };
            var defaults = new[] { "0", "0", "0" };
            for (int i = 0; i < 3; i++) {
                inputs.Controls.Add(MakeLabel(names[i]), i, 0);
                boxes[i].Dock = DockStyle.Fill;
                boxes[i].Text = defaults[i];
                boxes[i].BackColor = Color.FromArgb(46, 51, 39);
                boxes[i].ForeColor = Foreground;
                boxes[i].BorderStyle = BorderStyle.FixedSingle;
                boxes[i].Font = new Font("Consolas", 11F);
                boxes[i].Margin = new Padding(0, 0, 8, 0);
                inputs.Controls.Add(boxes[i], i, 1);
            }
            var formula = MakeLabel("Уравнение: ax^2 + bx + c = 0");
            inputs.Controls.Add(formula, 0, 2);
            inputs.SetColumnSpan(formula, 3);
            layout.Controls.Add(inputs, 0, 1);

            answerBox.Dock = DockStyle.Fill;
            answerBox.ReadOnly = true;
            answerBox.Multiline = true;
            answerBox.ScrollBars = ScrollBars.Vertical;
            answerBox.BorderStyle = BorderStyle.None;
            answerBox.BackColor = Surface;
            answerBox.ForeColor = Foreground;
            answerBox.Font = new Font("Consolas", 12F);
            answerBox.Text = "Введите коэффициенты и нажмите «Рассчитать».";
            var result = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(15) };
            result.Controls.Add(answerBox);
            layout.Controls.Add(result, 0, 2);
            Controls.Add(layout);
        }

        private static double ReadNumber(TextBox box, string name)
        {
            if (!double.TryParse(box.Text.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double number) || !double.IsFinite(number))
                throw new ArgumentException($"Коэффициент {name} должен быть числом.");
            return number;
        }

        private static string Format(double x) => x.ToString("G12", CultureInfo.CurrentCulture);

        private void Calculate() {
            try {
                double a = ReadNumber(aBox, "a");
                double b = ReadNumber(bBox, "b");
                double c = ReadNumber(cBox, "c");
                if (a == 0) throw new ArgumentException("a не должен равняться нулю.");

                double d = b * b - 4 * a * c;
                if (!double.IsFinite(d)) throw new ArgumentException("Слишком большие коэффициенты.");

                string text = $"D = b^2 - 4ac = {Format(d)}\r\n\r\n";
                if (d < 0) {
                    text += "Действительных корней нет.\r\n\r\n";
                    text += $"По Виету (для комплексных корней):\r\nx1 + x = {Format(-b / a)}\r\nx1 · x2 = {Format(c / a)}";
                } else {
                    double sqrtD = Math.Sqrt(d);
                    double x1 = (-b - sqrtD) / (2 * a);
                    double x2 = (-b + sqrtD) / (2 * a);
                    if (!double.IsFinite(x1) || !double.IsFinite(x2))
                        throw new ArgumentException("Не удалось вычислить корни: слишком большие числа.");
                    text += d == 0 ? $"Один корень: x = {Format(x1)}\r\n\r\n" :
                        $"x1 = {Format(x1)}\r\nx2 = {Format(x2)}\r\n\r\n";
                    text += "Теорема Виета:\r\n";
                    text += $"x1 + x2 = {Format(x1 + x2)}    (−b/a = {Format(-b / a)})\r\n";
                    text += $"x1 · x2 = {Format(x1 * x2)}    (c/a = {Format(c / a)})";
                }
                answerBox.Text = text;
            }
            catch (ArgumentException ex) {
                answerBox.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Clear() {
            aBox.Clear();
            bBox.Clear();
            cBox.Clear();
            answerBox.Clear();
            aBox.Focus();
        }
    }
}