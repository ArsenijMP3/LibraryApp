using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Database;
using LibraryApp.Models;

namespace LibraryApp.Forms
{
    public partial class BookListForm : UserControl
    {
        private User _currentUser;
        private DatabaseHelper _dbHelper;
        private bool _isLoaded = false;

        public BookListForm(User user, DatabaseHelper dbHelper)
        {
            InitializeComponent();
            _currentUser = user;
            _dbHelper = dbHelper;
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dataGridViewBooks.AutoGenerateColumns = false;
            dataGridViewBooks.Columns.Clear();
            
            dataGridViewBooks.Columns.Add("Title", "Название");
            dataGridViewBooks.Columns.Add("Author", "Автор");
            dataGridViewBooks.Columns.Add("Genre", "Жанр");
            dataGridViewBooks.Columns.Add("Year", "Год");
            dataGridViewBooks.Columns.Add("Publisher", "Издательство");
            dataGridViewBooks.Columns.Add("Copies", "Кол-во экз.");
            dataGridViewBooks.Columns.Add("Description", "Описание");
            dataGridViewBooks.Columns.Add("Status", "Статус");

            dataGridViewBooks.Columns["Title"].Width = 150;
            dataGridViewBooks.Columns["Author"].Width = 120;
            dataGridViewBooks.Columns["Genre"].Width = 100;
            dataGridViewBooks.Columns["Year"].Width = 60;
            dataGridViewBooks.Columns["Publisher"].Width = 120;
            dataGridViewBooks.Columns["Copies"].Width = 80;
            dataGridViewBooks.Columns["Description"].Width = 200;
            dataGridViewBooks.Columns["Status"].Width = 100;

            dataGridViewBooks.BackgroundColor = Color.White;
            dataGridViewBooks.BorderStyle = BorderStyle.None;
            dataGridViewBooks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewBooks.GridColor = Color.LightGray;
            dataGridViewBooks.RowTemplate.Height = 35;
            dataGridViewBooks.RowHeadersVisible = false;
            dataGridViewBooks.AllowUserToAddRows = false;
            dataGridViewBooks.AllowUserToDeleteRows = false;
            dataGridViewBooks.ReadOnly = true;
            dataGridViewBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBooks.MultiSelect = false;

            dataGridViewBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dataGridViewBooks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            dataGridViewBooks.EnableHeadersVisualStyles = false;

            foreach (DataGridViewColumn col in dataGridViewBooks.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            dataGridViewBooks.Sorted += (s, e) => 
            {
                if (_isLoaded) ApplyRowStyles();
            };

            dataGridViewBooks.DataBindingComplete += (s, e) =>
            {
                if (_isLoaded) ApplyRowStyles();
            };
        }

        private void ApplyRowStyles()
        {
            if (dataGridViewBooks.Rows == null || dataGridViewBooks.Rows.Count == 0) return;
            if (!dataGridViewBooks.Columns.Contains("Status") || !dataGridViewBooks.Columns.Contains("Copies")) return;

            try
            {
                foreach (DataGridViewRow row in dataGridViewBooks.Rows)
                {
                    if (row.IsNewRow) continue;
                    
                    string status = row.Cells["Status"]?.Value?.ToString() ?? "";
                    int copies = 0;
                    int.TryParse(row.Cells["Copies"]?.Value?.ToString(), out copies);

                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;

                    if (copies == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (status == "Выдана")
                    {
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                    }
                    else if (status == "В наличии")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(46, 139, 87);
                        row.DefaultCellStyle.ForeColor = Color.White;
                    }
                }
            }
            catch { }
        }

        public void LoadBooks()
        {
            try
            {
                var books = _dbHelper.GetBooks();
                dataGridViewBooks.Rows.Clear();
                _isLoaded = false;

                if (books == null || books.Count == 0)
                {
                    dataGridViewBooks.Rows.Add("Нет данных", "", "", "", "", "", "", "");
                    dataGridViewBooks.Rows[0].DefaultCellStyle.BackColor = Color.LightGray;
                    dataGridViewBooks.Rows[0].DefaultCellStyle.ForeColor = Color.Gray;
                    _isLoaded = true;
                    return;
                }

                foreach (var book in books)
                {
                    dataGridViewBooks.Rows.Add(
                        book.Title ?? "Без названия",
                        book.Author ?? "Неизвестен",
                        book.Genre ?? "Не указан",
                        book.Year > 0 ? book.Year.ToString() : "—",
                        book.Publisher ?? "Не указано",
                        book.Copies.ToString(),
                        book.Description ?? "",
                        book.Status ?? "Неизвестно"
                    );
                }

                _isLoaded = true;
                ApplyRowStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isLoaded = true;
            }
        }

        public void RefreshBooks()
        {
            LoadBooks();
        }
    }
}