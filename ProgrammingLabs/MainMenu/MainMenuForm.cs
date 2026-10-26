using System;
using System.Drawing;
using System.Windows.Forms;
using ProgrammingLabs.Labs.Dichotomy;
using ProgrammingLabs.Labs.GoldenSection;
using ProgrammingLabs.Labs.Sortix;

namespace ProgrammingLabs.MainMenu
{
    public partial class MainMenuForm : Form
    {
        private static readonly Color Bg = Color.FromArgb(49, 53, 39);
        private static readonly Color PanelColor = Color.FromArgb(60, 65, 48);
        private static readonly Color Border = Color.FromArgb(112, 120, 91);
        private static readonly Color TextColor = Color.FromArgb(215, 215, 190);
        private static readonly Color Accent = Color.FromArgb(172, 185, 132);

        public MainMenuForm()
        {
            InitializeComponent();
            Text = "Programming Labs";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(470, 395);
            Size = new Size(640, 470);
            BackColor = Bg;
            Font = new Font("Tahoma", 10F);
            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) Open(() => new SortixForm());
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) Open(() => new DichotomyForm());
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) Open(() => new GoldenSectionForm());
                if (e.KeyCode == Keys.Escape) Close();
            };
            BuildInterface();
        }

        private static Label Label(string text, Font font, Color color) => new Label
        {
            Text = text, Font = font, ForeColor = color, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private Button Button(string text, Action action)
        {
            var button = new Button
            {
                Text = text, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                BackColor = PanelColor, ForeColor = TextColor, Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Tahoma", 11F), Padding = new Padding(15, 0, 0, 0)
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(84, 96, 69);
            button.Click += (_, _) => action();
            return button;
        }

        private void BuildInterface()
        {
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                Padding = new Padding(12), BackColor = Bg
            };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            var title = Label(" Console    /    Programming Labs", new Font("Tahoma", 11, FontStyle.Bold), TextColor);
            title.BackColor = Color.FromArgb(83, 91, 70);
            outer.Controls.Add(title, 0, 0);

            var list = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5,
                BackColor = PanelColor, Padding = new Padding(15), Margin = new Padding(0, 8, 0, 8)
            };
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            list.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            list.Controls.Add(Label("Выберите программу", new Font("Tahoma", 10), Accent), 0, 0);
            list.Controls.Add(Button("1    Sortix", () => Open(() => new SortixForm())), 0, 1);
            list.Controls.Add(Button("2    Dichotomy", () => Open(() => new DichotomyForm())), 0, 2);
            list.Controls.Add(Button("3    Golden Section", () => Open(() => new GoldenSectionForm())), 0, 3);
            outer.Controls.Add(list, 0, 1);

            var close = Button("Закрыть", Close);
            close.Margin = new Padding(0, 2, 0, 2);
            outer.Controls.Add(close, 0, 2);
            outer.Controls.Add(Label("  1 / 2 / 3 — открыть     Esc — выход", new Font("Tahoma", 8), Accent), 0, 3);
            Controls.Add(outer);
        }

        private void Open(Func<Form> create)
        {
            Hide();
            try { using (var window = create()) window.ShowDialog(); }
            finally { if (!IsDisposed) { Show(); Activate(); } }
        }
    }
}