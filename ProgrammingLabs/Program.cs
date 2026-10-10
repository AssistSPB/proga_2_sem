using System;
using System.Windows.Forms;
using ProgrammingLabs.MainMenu;

namespace ProgrammingLabs
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainMenuForm());
        }
    }
}
