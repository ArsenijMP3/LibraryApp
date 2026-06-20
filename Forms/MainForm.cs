using System;
using System.Windows.Forms;
using LibraryApp.Database;
using LibraryApp.Models;

namespace LibraryApp.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private DatabaseHelper _dbHelper;
        private BookListForm _bookListForm;

        public MainForm(User user, DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _currentUser = user;
            _dbHelper = dbHelper;
            
            labelUser.Text = $"👤 {_currentUser.FullName}";
            
            // Подписываем события
            buttonLogout.Click += ButtonLogout_Click;
            buttonViewBooks.Click += ButtonViewBooks_Click;
            buttonViewIssues.Click += ButtonViewIssues_Click;
            this.FormClosing += MainForm_FormClosing;
            
            ConfigureButtons();
            LoadBooks();
        }

        private void ConfigureButtons()
        {
            buttonViewBooks.Visible = true;
            buttonViewIssues.Visible = _dbHelper.CanViewIssues(_currentUser);
        }

        private void LoadBooks()
        {
            if (_bookListForm != null)
            {
                panelContent.Controls.Remove(_bookListForm);
                _bookListForm.Dispose();
                _bookListForm = null;
            }

            _bookListForm = new BookListForm(_currentUser, _dbHelper);
            _bookListForm.TopLevel = false;
            _bookListForm.FormBorderStyle = FormBorderStyle.None;
            _bookListForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(_bookListForm);
            _bookListForm.Show();
            _bookListForm.LoadBooks();
        }

        private void ButtonViewBooks_Click(object sender, EventArgs e)
        {
            LoadBooks();
        }

        private void ButtonViewIssues_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Форма выдачи книг будет реализована в Модуле 3",
                           "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_bookListForm != null)
            {
                panelContent.Controls.Remove(_bookListForm);
                _bookListForm.Dispose();
                _bookListForm = null;
            }
        }
    }
}