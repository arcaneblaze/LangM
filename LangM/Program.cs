using System;
using System.Windows.Forms;

namespace LangM;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new KeyboardLanguageTracker());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Критическая ошибка: {ex.Message}");
        }
    }
}