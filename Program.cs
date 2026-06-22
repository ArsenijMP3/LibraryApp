using System;
using System.Windows.Forms;
using LibraryApp.Forms;

namespace LibraryApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Цикл "окно входа -> главный экран -> (логаут) -> снова окно входа -> ... -> выход".
            // Каждая форма создаётся и корректно освобождается (using), утечек скрытых форм нет.
            while (true)
            {
                string fio;
                string roleName;
                int roleId;

                using (var loginForm = new LoginForm())
                {
                    // Окно входа — первое, что видит пользователь (Задание 2.3)
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        break; // окно входа закрыто крестиком -> выход из приложения
                    }

                    fio = loginForm.LoggedInFio;
                    roleName = loginForm.LoggedInRoleName;
                    roleId = loginForm.LoggedInRoleId;
                }

                using var mainForm = new MainForm(fio, roleName, roleId);
                if (mainForm.ShowDialog() != DialogResult.OK)
                {
                    break; // главное окно закрыто крестиком -> выход из приложения
                }
                // DialogResult.OK от MainForm означает "Выход" -> по циклу возвращаемся к окну входа
            }
        }
    }
}
