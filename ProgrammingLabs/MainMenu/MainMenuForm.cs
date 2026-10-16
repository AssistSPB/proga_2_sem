using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ProgrammingLabs.Labs.Sortix;
using ProgrammingLabs.Labs.Dichotomy;

namespace ProgrammingLabs.MainMenu
{
    public partial class MainMenuForm : Form
    {

        private static readonly Color Background = Color.FromArgb(245, 248, 247);
        private static readonly Color Dark = Color.FromArgb(27, 65, 72);
        private static readonly Color Darker = Color.FromArgb(20, 50, 57);
        private static readonly Color Accent = Color.FromArgb(32, 138, 129);
        private static readonly Color Muted = Color.FromArgb(111, 135, 138);
        private static readonly Color Stroke = Color.FromArgb(222, 233, 230);
        private static readonly Color Violet = Color.FromArgb(125, 100, 177);


        public MainMenuForm()
        {
            InitializeComponent();
            BuildInterface();
        }


        private void BuildInterface()
        {
            Text = "Programming Labs — Главное меню";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(850, 580);
            Size = new Size(1120, 720);
            Font = new Font("Segoe UI", 10F);
            BackColor = Background;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Background,
                Padding = new Padding(28, 25, 28, 16)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

             
            var hero = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Dark,
                Padding = new Padding(32, 20, 30, 18),
                Margin = new Padding(0, 0, 0, 16)
            };
            var heroText = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 61));
            heroText.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            heroText.Controls.Add(MakeLabel("C#   /   ПРАКТИКУМ ПО ПРОГРАММИРОВАНИЮ", 10,
                Color.FromArgb(144, 221, 207), true), 0, 0);
            heroText.Controls.Add(MakeLabel("Лабораторные работы", 28, Color.White, true), 0, 1);
            heroText.Controls.Add(MakeLabel("Два практических приложения. Выбери, с чего начать.", 11,
                Color.FromArgb(197, 217, 214)), 0, 2);
            hero.Controls.Add(heroText);

             
            var cards = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Background
            };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            cards.Controls.Add(CreateCard(
                "01  /  SORTIX", "Алгоритмы сортировки",
                "Визуализация и сравнение пяти алгоритмов сортировки.",
                "Генерация данных  ·  Графики  ·  Отчёт",
                Violet, Sortix_Click, new Padding(0, 0, 9, 0)), 0, 0);

            cards.Controls.Add(CreateCard(
                "02  /  DICHOTOMY", "Метод дихотомии",
                "Поиск корня уравнения f(x) = 0 методом половинного деления.",
                "Ввод формулы  ·  График  ·  Точность",
                Accent, Dichotomy_Click, new Padding(9, 0, 0, 0)), 1, 0);

            var footer = MakeLabel("PROGRAMMING LABS     •     WINDOWS FORMS", 9, Muted, true);
            footer.TextAlign = ContentAlignment.MiddleCenter;

            layout.Controls.Add(hero, 0, 0);
            layout.Controls.Add(cards, 0, 1);
            layout.Controls.Add(footer, 0, 2);
            Controls.Add(layout);
        }

        private static Panel CreateCard(string number, string title, string description,
            string details, Color tint, EventHandler open, Padding margin)
        {
            var card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = margin,
                Padding = new Padding(28, 25, 28, 24),
                BorderColor = Stroke
            };
            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

            var tag = MakeLabel(number, 10, tint, true);
            var heading = MakeLabel(title, 19, Dark, true);
            var info = MakeLabel(description, 11, Darker);
            var subtle = MakeLabel(details, 9, Muted);
            subtle.TextAlign = ContentAlignment.BottomLeft;
            var openButton = new ActionButton(tint)
            {
                Dock = DockStyle.Fill,
                Text = "Открыть лабораторную   →"
            };
            openButton.Click += open;

            content.Controls.Add(tag, 0, 0);
            content.Controls.Add(heading, 0, 1);
            content.Controls.Add(info, 0, 2);
            content.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White }, 0, 3);
            content.Controls.Add(subtle, 0, 4);
            content.Controls.Add(openButton, 0, 5);
            card.Controls.Add(content);
            return card;
        }

        private static Label MakeLabel(string text, float fontSize, Color color, bool bold = false)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                ForeColor = color,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

         
        private sealed class RoundedPanel : Panel
        {
             
            public Color BorderColor = Color.Transparent;

            public RoundedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (Width < 2 || Height < 2) return;
                using (GraphicsPath path = RoundPath(new Rectangle(0, 0, Width, Height), 16))
                {
                    var previous = Region;
                    Region = new Region(path);
                    previous?.Dispose();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (BorderColor == Color.Transparent || Width < 3 || Height < 3) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundPath(new Rectangle(1, 1, Width - 2, Height - 2), 15);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private sealed class ActionButton : Button
        {
            private readonly Color accent;
            private bool hovered;

            public ActionButton(Color color)
            {
                accent = color;
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Cursor = Cursors.Hand;
                Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                ForeColor = Color.White;
            }

            protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Width < 4 || Height < 4) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(Parent?.BackColor ?? Color.White);
                using var path = RoundPath(new Rectangle(1, 1, Width - 2, Height - 2), 10);
                using var brush = new SolidBrush(hovered ? ControlPaint.Dark(accent, 0.12F) : accent);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if (Focused && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -6, -6));
            }
        }

        private static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            float d = Math.Min(radius * 2F, Math.Min(r.Width, r.Height));
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }



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
