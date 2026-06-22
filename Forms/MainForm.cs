using System;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Forms
{
    /// <summary>
    /// Главный экран — каталог книг.
    /// Гость/читатель — только просмотр (Задание 2.4).
    /// Библиотекарь — + поиск/фильтр/сортировка в реальном времени (Задание 3.2).
    /// Администратор — + добавление/редактирование/удаление (Задание 3.3, 3.4).
    /// </summary>
    public class MainForm : Form
    {
        private readonly string _fio;
        private readonly string _roleName;
        private readonly int _roleId;

        private readonly Panel _topPanel = new();
        private readonly Label _lblUser = new();
        private readonly Button _btnLogout = new();

        // --- панель поиска/фильтра/сортировки (3.2) — видна библиотекарю и администратору ---
        private readonly Panel _toolPanel = new();
        private readonly TextBox _txtSearch = new();
        private readonly ComboBox _cmbGenre = new();
        private readonly ComboBox _cmbSort = new();

        // --- панель CRUD (3.3, 3.4) — видна только администратору ---
        private readonly Panel _crudPanel = new();
        private readonly Button _btnAdd = new();
        private readonly Button _btnEdit = new();
        private readonly Button _btnDelete = new();

        private readonly DataGridView _grid = new();

        private bool _isLoading; // защита от рекурсивных Search-событий при программном заполнении комбобоксов

        // Цвета согласно Заданию 2.4
        private static readonly Color ColorOutOfStock = Color.FromArgb(220, 53, 69);   // 0 экземпляров — красный
        private static readonly Color ColorAvailable = Color.FromArgb(46, 139, 87);    // в наличии — #2E8B57
        private static readonly Color ColorIssued = Color.FromArgb(173, 216, 230);     // выдана — голубой

        private bool CanSearchFilterSort => _roleId == RoleIds.Librarian || _roleId == RoleIds.Admin;
        private bool CanManageBooks => _roleId == RoleIds.Admin;

        public MainForm(string fio, string roleName, int roleId)
        {
            _fio = fio;
            _roleName = roleName;
            _roleId = roleId;
            InitializeComponent();
            RefreshGrid();
        }

        private void InitializeComponent()
        {
            Text = $"Библиотека — Каталог книг ({_roleName})";
            ClientSize = new Size(1100, 640);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(900, 520);

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

            // --- панель поиска/фильтра/сортировки (3.2) ---
            _toolPanel.Dock = DockStyle.Top;
            _toolPanel.Height = 46;
            _toolPanel.Padding = new Padding(10, 8, 10, 8);
            _toolPanel.Visible = CanSearchFilterSort;

            var lblSearch = new Label { Text = "Поиск:", Font = Font, AutoSize = true, Location = new Point(10, 13) };
            _txtSearch.Location = new Point(lblSearch.Right + 10, 9);
            _txtSearch.Size = new Size(220, 25);
            _txtSearch.TextChanged += (_, _) => RefreshGrid(); // в реальном времени, без кнопок

            var lblGenre = new Label { Text = "Жанр:", Font = Font, AutoSize = true, Location = new Point(_txtSearch.Right + 20, 13) };
            _cmbGenre.Location = new Point(lblGenre.Right + 10, 9);
            _cmbGenre.Size = new Size(170, 25);
            _cmbGenre.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbGenre.SelectedIndexChanged += (_, _) => RefreshGrid();

            var lblSort = new Label { Text = "Сортировка:", Font = Font, AutoSize = true, Location = new Point(_cmbGenre.Right + 20, 13) };
            _cmbSort.Location = new Point(lblSort.Right + 10, 9);
            _cmbSort.Size = new Size(220, 25);
            _cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbSort.Items.AddRange(new object[]
            {
                "По названию (А-Я)",
                "Год издания (по возрастанию)",
                "Год издания (по убыванию)"
            });
            _cmbSort.SelectedIndex = 0;
            _cmbSort.SelectedIndexChanged += (_, _) => RefreshGrid(); // сортировка сохраняется вместе с поиском/фильтром

            _toolPanel.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblGenre, _cmbGenre, lblSort, _cmbSort });

            // --- панель CRUD (3.3, 3.4) — только администратор ---
            _crudPanel.Dock = DockStyle.Top;
            _crudPanel.Height = 44;
            _crudPanel.Padding = new Padding(10, 6, 10, 6);
            _crudPanel.Visible = CanManageBooks;

            _btnAdd.Text = "Добавить";
            _btnAdd.Location = new Point(10, 6);
            _btnAdd.Size = new Size(110, 30);
            _btnAdd.FlatStyle = FlatStyle.Flat;
            _btnAdd.BackColor = Color.FromArgb(46, 139, 87);
            _btnAdd.ForeColor = Color.White;
            _btnAdd.Click += BtnAdd_Click;

            _btnEdit.Text = "Редактировать";
            _btnEdit.Location = new Point(130, 6);
            _btnEdit.Size = new Size(120, 30);
            _btnEdit.FlatStyle = FlatStyle.Flat;
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete.Text = "Удалить";
            _btnDelete.Location = new Point(260, 6);
            _btnDelete.Size = new Size(110, 30);
            _btnDelete.FlatStyle = FlatStyle.Flat;
            _btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            _btnDelete.ForeColor = Color.White;
            _btnDelete.Click += BtnDelete_Click;

            _crudPanel.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete });

            // --- таблица книг ---
            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;
            _grid.CellFormatting += Grid_CellFormatting;
            if (CanManageBooks)
            {
                _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) OpenEditForm(); };
            }

            // Порядок добавления важен для Dock: Fill добавляем первым, потом верхние панели сверху вниз.
            Controls.Add(_grid);
            Controls.Add(_crudPanel);
            Controls.Add(_toolPanel);
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

        /// <summary>
        /// Перечитывает книги с учётом роли: гость/читатель — без фильтров (2.4),
        /// библиотекарь/администратор — поиск + фильтр по жанру + сортировка по году,
        /// всё применяется совместно и в реальном времени (3.2).
        /// </summary>
        private void RefreshGrid()
        {
            if (_isLoading)
            {
                return;
            }

            try
            {
                System.Collections.Generic.List<BookRow> books;

                if (CanSearchFilterSort)
                {
                    try
                    {
                        EnsureGenreListLoaded();
                    }
                    catch (Exception genreEx)
                    {
                        // Не блокируем отображение каталога, если справочник жанров не загрузился —
                        // фильтр по жанру в этом случае просто не сработает, но список книг покажется.
                        MessageBox.Show(
                            $"Не удалось загрузить список жанров для фильтра:\n{genreEx.Message}",
                            "Предупреждение",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }

                    BookSortMode sortMode = _cmbSort.SelectedIndex switch
                    {
                        1 => BookSortMode.ByYearAsc,
                        2 => BookSortMode.ByYearDesc,
                        _ => BookSortMode.ByTitle
                    };

                    string genre = _cmbGenre.SelectedItem?.ToString() ?? BookService.AllGenresLabel;
                    books = BookService.SearchBooks(_txtSearch.Text.Trim(), genre, sortMode);
                }
                else
                {
                    books = BookService.GetAllBooks();
                }

                FillGrid(books);
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

        private void EnsureGenreListLoaded()
        {
            if (_cmbGenre.Items.Count > 0)
            {
                return;
            }

            _isLoading = true;
            try
            {
                string previousSelection = _cmbGenre.SelectedItem?.ToString() ?? BookService.AllGenresLabel;

                _cmbGenre.Items.Clear();
                foreach (string genre in BookService.GetGenresForFilter()) // "Все жанры" — первым элементом
                {
                    _cmbGenre.Items.Add(genre);
                }

                int index = _cmbGenre.Items.IndexOf(previousSelection);
                _cmbGenre.SelectedIndex = index >= 0 ? index : 0;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void FillGrid(System.Collections.Generic.List<BookRow> books)
        {
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
                    book.Title,
                    book.Author,
                    book.Genre,
                    (object?)book.Year ?? DBNull.Value,
                    (object?)book.Publisher ?? DBNull.Value,
                    book.Copies,
                    (object?)book.Description ?? DBNull.Value,
                    book.Availability);

                _grid.Rows[rowIndex].Tag = book;
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

        private BookRow? GetSelectedBook()
        {
            if (_grid.CurrentRow?.Tag is BookRow book)
            {
                return book;
            }
            return null;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            // BookEditForm открывается строго модально (ShowDialog) — пока он открыт,
            // это окно недоступно для ввода, поэтому второе окно редактирования открыть нельзя.
            using var form = new BookEditForm(isEdit: false, existing: null);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                RefreshGrid(); // автообновление списка (3.4)
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e) => OpenEditForm();

        private void OpenEditForm()
        {
            BookRow? selected = GetSelectedBook();
            if (selected == null)
            {
                MessageBox.Show(
                    "Выберите книгу в списке для редактирования.",
                    "Книга не выбрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var form = new BookEditForm(isEdit: true, existing: selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                RefreshGrid();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            BookRow? selected = GetSelectedBook();
            if (selected == null)
            {
                MessageBox.Show(
                    "Выберите книгу в списке для удаления.",
                    "Книга не выбрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Запрет удаления при наличии связанных записей (Задание 3.4)
                int relatedLoans = BookService.CountRelatedLoans(selected.Id);
                if (relatedLoans > 0)
                {
                    MessageBox.Show(
                        $"Невозможно удалить книгу «{selected.Title}» — с ней связано {relatedLoans} " +
                        "запись(ей) в таблице выдач. Сначала удалите или измените связанные выдачи.",
                        "Удаление запрещено",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirm = MessageBox.Show(
                    $"Удалить книгу «{selected.Title}»?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                BookService.DeleteBook(selected.Id);
                PhotoService.DeletePhoto(selected.PhotoPath);

                RefreshGrid(); // автообновление списка (3.4)
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить книгу:\n{ex.Message}",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
