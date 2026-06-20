using System;
using System.Windows.Forms;
using LibraryApp.Forms;

namespace LibraryApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n" +
                               "Приложение будет закрыто.",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}