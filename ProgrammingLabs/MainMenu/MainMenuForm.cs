using System;
using System.Drawing;
using System.Windows.Forms;
using ProgrammingLabs.Labs.Sortix;
using ProgrammingLabs.Labs.Dichotomy;

namespace ProgrammingLabs.MainMenu
{
    public partial class MainMenuForm : Form
    {
        // 1. Цвета главного меню — те же спокойные оттенки, что и в дихотомии.
        private static readonly Color Background = Color.FromArgb(238, 245, 242);
        private static readonly Color White = Color.White;
        private static readonly Color DarkTeal = Color.FromArgb(32, 86, 91);
        private static readonly Color Accent = Color.FromArgb(41, 148, 143);
        private static readonly Color AccentHover = Color.FromArgb(30, 125, 122);
        private static readonly Color Muted = Color.FromArgb(105, 130, 128);
        private static readonly Color Border = Color.FromArgb(209, 229, 223);

        public MainMenuForm()
        {
            InitializeComponent(); // Стандартный метод WinForms из Designer.
            BuildInterface();      // Сам интерфейс создаём здесь, как в Sortix.
        }

        // 2. Построение окна и расположение блоков.
        private void BuildInterface()
        {
            Text = "Programming Labs — Главное меню";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(770, 490);
            Size = new Size(1060, 650);
            BackColor = Background;
            Font = new Font("Segoe UI", 10F);

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Background,
                Padding = new Padding(32, 25, 32, 17),
                ColumnCount = 1,
                RowCount = 3
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 138F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            var header = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 53F));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            header.Controls.Add(MakeLabel("C#  /  WINDOWS FORMS", 9, Accent, true), 0, 0);
            header.Controls.Add(MakeLabel("Лабораторные работы", 27, DarkTeal, true), 0, 1);
            header.Controls.Add(MakeLabel("Выберите лабораторную работу, чтобы начать", 11, Muted), 0, 2);

            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 7, 0, 12)
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cards.Controls.Add(CreateCard(
                "01 / SORTIX", "Алгоритмы сортировки",
                "Пузырьковая, вставками, шейкерная, быстрая и BOGO.",
                "Визуализация, настройки и сравнение алгоритмов.",
                Sortix_Click, new Padding(0, 0, 9, 0)), 0, 0);

            cards.Controls.Add(CreateCard(
                "02 / DICHOTOMY", "Метод дихотомии",
                "Поиск корня уравнения f(x) = 0.",
                "Ввод формулы, построение графика и точность поиска.",
                Dichotomy_Click, new Padding(9, 0, 0, 0)), 1, 0);

            var footer = MakeLabel("PROGRAMMING LABS   •   Главное меню", 9, Muted);
            footer.TextAlign = ContentAlignment.MiddleCenter;

            page.Controls.Add(header, 0, 0);
            page.Controls.Add(cards, 0, 1);
            page.Controls.Add(footer, 0, 2);
            Controls.Add(page);
        }

        // 3. Одинаковое оформление карточек лабораторных.
        private static Panel CreateCard(string number, string title, string description,
            string note, EventHandler open, Padding margin)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = White,
                Padding = new Padding(25),
                Margin = margin
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            content.Controls.Add(MakeLabel(number, 10, Accent, true), 0, 0);
            content.Controls.Add(MakeLabel(title, 18, DarkTeal, true), 0, 1);
            content.Controls.Add(MakeLabel(description, 11, DarkTeal), 0, 2);
            content.Controls.Add(MakeLabel(note, 10, Muted), 0, 3);

            var button = new Button
            {
                Text = "Открыть лабораторную  →",
                Dock = DockStyle.Fill,
                BackColor = Accent,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += (s, e) => button.BackColor = AccentHover;
            button.MouseLeave += (s, e) => button.BackColor = Accent;
            button.Click += open;
            content.Controls.Add(button, 0, 4);

            card.Controls.Add(content);
            return card;
        }

        private static Label MakeLabel(string text, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        // 4. Открытие Sortix. После закрытия лабораторной меню снова появляется.
        private void Sortix_Click(object? sender, EventArgs e)
        {
            Hide();
            try
            {
                using var form = new SortixForm();
                form.ShowDialog();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        // 5. Открытие дихотомии. После «В меню» возвращаемся сюда.
        private void Dichotomy_Click(object? sender, EventArgs e)
        {
            Hide();
            try
            {
                using var form = new DichotomyForm();
                form.ShowDialog();
            }
            finally
            {
                Show();
                Activate();
            }
        }
    }
}
