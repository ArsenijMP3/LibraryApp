using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Forms
{
    /// <summary>
    /// Окно входа. Первое окно приложения (Задание 2.3).
    /// Единый стиль оформления и нейминг см. Задание 2.2.
    /// Используется через ShowDialog(): успешный вход/гость -> DialogResult.OK,
    /// закрытие крестиком -> DialogResult.Cancel (приложение завершится).
    /// </summary>
    public class LoginForm : Form
    {
        private readonly TextBox _txtLogin = new();
        private readonly TextBox _txtPassword = new();
        private readonly Button _btnLogin = new();
        private readonly Button _btnGuest = new();
        private readonly Label _lblError = new();

        /// <summary>ФИО пользователя, под которым выполнен вход (заполняется после успешного входа).</summary>
        public string LoggedInFio { get; private set; } = "";

        /// <summary>Название роли пользователя (заполняется после успешного входа).</summary>
        public string LoggedInRoleName { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Библиотека — Вход";
            ClientSize = new Size(380, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;

            var lblLogo = new Label
            {
                Text = "📚  Система учёта книг в библиотеке",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 47, 61),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 20),
                Size = new Size(380, 50)
            };

            var lblLogin = new Label { Text = "Логин:", Location = new Point(50, 95), AutoSize = true };
            _txtLogin.Location = new Point(50, 115);
            _txtLogin.Size = new Size(280, 25);

            var lblPassword = new Label { Text = "Пароль:", Location = new Point(50, 150), AutoSize = true };
            _txtPassword.Location = new Point(50, 170);
            _txtPassword.Size = new Size(280, 25);
            _txtPassword.UseSystemPasswordChar = true;

            _btnLogin.Text = "Войти";
            _btnLogin.Location = new Point(50, 210);
            _btnLogin.Size = new Size(135, 32);
            _btnLogin.BackColor = Color.FromArgb(46, 139, 87);
            _btnLogin.ForeColor = Color.White;
            _btnLogin.FlatStyle = FlatStyle.Flat;
            _btnLogin.Click += BtnLogin_Click;

            _btnGuest.Text = "Войти как гость";
            _btnGuest.Location = new Point(195, 210);
            _btnGuest.Size = new Size(135, 32);
            _btnGuest.FlatStyle = FlatStyle.Flat;
            _btnGuest.Click += BtnGuest_Click;

            _lblError.ForeColor = Color.Red;
            _lblError.Location = new Point(50, 255);
            _lblError.Size = new Size(280, 30);
            _lblError.TextAlign = ContentAlignment.MiddleCenter;

            Controls.AddRange(new Control[]
            {
                lblLogo, lblLogin, _txtLogin, lblPassword, _txtPassword,
                _btnLogin, _btnGuest, _lblError
            });

            AcceptButton = _btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string login = _txtLogin.Text.Trim();
            string password = _txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                _lblError.Text = "Введите логин и пароль";
                return;
            }

            try
            {
                Reader? reader = AuthService.TryLogin(login, password);

                if (reader == null)
                {
                    _lblError.Text = "Неверный логин или пароль";
                    return;
                }

                LoggedInFio = reader.Fio;
                LoggedInRoleName = reader.RoleName;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка подключения к базе данных:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnGuest_Click(object? sender, EventArgs e)
        {
            // Гость не проходит авторизацию — сразу переходит к просмотру каталога.
            LoggedInFio = "Гость";
            LoggedInRoleName = "гость";
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
