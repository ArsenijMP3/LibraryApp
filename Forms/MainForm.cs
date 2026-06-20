using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Forms
{
    /// <summary>
    /// Главный экран — каталог книг. Доступен всем ролям (Задание 2.3/2.4).
    /// Раскраска строк по правилам Задания 2.4.
    /// Поиск/сортировка/фильтрация и CRUD — реализуются в Модуле 3.
    /// </summary>
    public class MainForm : Form
    {
        private readonly string _fio;
        private readonly string _roleName;

        private readonly Panel _topPanel = new();
        private readonly Label _lblUser = new();
        private readonly Button _btnLogout = new();
        private readonly DataGridView _grid = new();

        // Цвета согласно Заданию 2.4
        private static readonly Color ColorOutOfStock = Color.FromArgb(220, 53, 69);   // 0 экземпляров — красный
        private static readonly Color ColorAvailable = Color.FromArgb(46, 139, 87);    // в наличии — #2E8B57
        private static readonly Color ColorIssued = Color.FromArgb(173, 216, 230);     // выдана — голубой

        public MainForm(string fio, string roleName)
        {
            _fio = fio;
            _roleName = roleName;
            InitializeComponent();
            LoadBooks();
        }

        private void InitializeComponent()
        {
            Text = $"Библиотека — Каталог книг ({_roleName})";
            ClientSize = new Size(1050, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(850, 500);

            // --- верхняя панель: ФИО пользователя справа (Задание 2.3) ---
            _topPanel.Dock = DockStyle.Top;
            _topPanel.Height = 50;
            _topPanel.BackColor = Color.FromArgb(33, 47, 61);

            _lblUser.Text = $"{_fio}   ({_roleName})";
            _lblUser.ForeColor = Color.White;
            _lblUser.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblUser.AutoSize = true;

            _btnLogout.Text = "Выход";
            _btnLogout.Size = new Size(90, 30);
            _btnLogout.FlatStyle = FlatStyle.Flat;
            _btnLogout.Click += BtnLogout_Click;

            _topPanel.Controls.Add(_lblUser);
            _topPanel.Controls.Add(_btnLogout);
            _topPanel.Resize += (_, _) => PositionTopPanelControls();
            Load += (_, _) => PositionTopPanelControls();

            // --- таблица книг ---
            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.CellFormatting += Grid_CellFormatting;

            Controls.Add(_grid);
            Controls.Add(_topPanel);
        }

        private void PositionTopPanelControls()
        {
            _btnLogout.Location = new Point(
                _topPanel.Width - _btnLogout.Width - 15,
                (_topPanel.Height - _btnLogout.Height) / 2);

            _lblUser.Location = new Point(
                _btnLogout.Left - _lblUser.Width - 20,
                (_topPanel.Height - _lblUser.Height) / 2);
        }

        private void LoadBooks()
        {
            try
            {
                var books = BookService.GetAllBooks();

                _grid.Columns.Clear();
                _grid.Columns.Add("Title", "Название");
                _grid.Columns.Add("Author", "Автор");
                _grid.Columns.Add("Genre", "Жанр");
                _grid.Columns.Add("Year", "Год издания");
                _grid.Columns.Add("Publisher", "Издательство");
                _grid.Columns.Add("Copies", "Кол-во экз.");
                _grid.Columns.Add("Description", "Описание");
                _grid.Columns.Add("Availability", "Наличие");

                _grid.Rows.Clear();

                foreach (BookRow book in books)
                {
                    int rowIndex = _grid.Rows.Add(
                        book.Title, book.Author, book.Genre, book.Year,
                        book.Publisher, book.Copies, book.Description, book.Availability);

                    _grid.Rows[rowIndex].Tag = book;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список книг:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (_grid.Rows[e.RowIndex].Tag is not BookRow book)
            {
                return;
            }

            DataGridViewRow row = _grid.Rows[e.RowIndex];

            if (book.Copies == 0)
            {
                row.DefaultCellStyle.BackColor = ColorOutOfStock;
                row.DefaultCellStyle.ForeColor = Color.White;
            }
            else if (book.Availability == "Выдана")
            {
                row.DefaultCellStyle.BackColor = ColorIssued;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = ColorAvailable;
                row.DefaultCellStyle.ForeColor = Color.White;
            }
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            // Возврат на главный экран — окно входа (Задание 2.3).
            // DialogResult.OK здесь означает "выйти из аккаунта" — Program.cs снова покажет LoginForm.
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
