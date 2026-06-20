using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Database;
using LibraryApp.Models;

namespace LibraryApp.Forms
{
    public partial class LoginForm : Form
    {
        private DatabaseHelper _dbHelper;
        
        public LoginForm()
        {
            InitializeComponent();
            
            // ⚠️ ИЗМЕНИТЕ ЭТОТ ПУТЬ НА СВОЙ!
            string dbPath = @"D:\LibraryApp\base.accdb";
            
            if (!System.IO.File.Exists(dbPath))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Access Database|*.accdb;*.mdb";
                    ofd.Title = "Выберите файл базы данных";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        dbPath = ofd.FileName;
                    }
                    else
                    {
                        MessageBox.Show("Файл базы данных не выбран. Приложение будет закрыто.", 
                                       "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
            }
            
            _dbHelper = new DatabaseHelper(dbPath);
            
            if (!_dbHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных.\n" +
                               "Проверьте, что файл не открыт в Access.",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            textBoxPassword.KeyPress += TextBoxPassword_KeyPress;
            textBoxLogin.KeyPress += TextBoxLogin_KeyPress;
            textBoxLogin.Focus();
            
            // Подписываем события
            buttonLogin.Click += ButtonLogin_Click;
            buttonGuest.Click += ButtonGuest_Click;
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string login = textBoxLogin.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                buttonLogin.Enabled = false;
                buttonGuest.Enabled = false;
                
                var user = _dbHelper.AuthenticateUser(login, password);
                
                if (user != null)
                {
                    labelError.Text = "";
                    this.Hide();
                    
                    using (var mainForm = new MainForm(user, _dbHelper))
                    {
                        mainForm.ShowDialog();
                    }
                    
                    this.Show();
                    textBoxLogin.Text = "";
                    textBoxPassword.Text = "";
                    labelError.Text = "";
                    textBoxLogin.Focus();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    textBoxPassword.Text = "";
                    textBoxPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                buttonLogin.Enabled = true;
                buttonGuest.Enabled = true;
            }
        }

        private void ButtonGuest_Click(object sender, EventArgs e)
        {
            var guestUser = new User
            {
                Id = 0,
                FullName = "Гость",
                Login = "",
                Password = "",
                RoleId = 0,
                RoleName = "гость",
                Contact = ""
            };

            this.Hide();
            
            using (var mainForm = new MainForm(guestUser, _dbHelper))
            {
                mainForm.ShowDialog();
            }
            
            this.Show();
            textBoxLogin.Text = "";
            textBoxPassword.Text = "";
            labelError.Text = "";
        }

        private void TextBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ButtonLogin_Click(sender, e);
            }
        }

        private void TextBoxLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textBoxPassword.Focus();
            }
        }

        private void ShowError(string message)
        {
            labelError.Text = message;
            labelError.ForeColor = Color.Red;
            labelError.Visible = true;
        }
    }
}