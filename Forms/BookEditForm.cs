using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Forms
{
    /// <summary>
    /// Отдельное окно добавления/редактирования книги (Задание 3.3).
    /// Открывается строго модально (ShowDialog) — пока окно открыто, MainForm заблокирован
    /// средствами Windows, поэтому второе такое окно открыть невозможно (требование
    /// "запрет нескольких окон редактирования одновременно" выполняется автоматически).
    /// </summary>
    public class BookEditForm : Form
    {
        private readonly bool _isEdit;
        private readonly BookRow? _existing;

        private readonly TextBox _txtId = new() { ReadOnly = true };
        private readonly TextBox _txtTitle = new();
        private readonly TextBox _txtAuthor = new();
        private readonly ComboBox _cmbGenre = new() { DropDownStyle = ComboBoxStyle.DropDown };
        private readonly NumericUpDown _numYear = new();
        private readonly TextBox _txtPublisher = new();
        private readonly NumericUpDown _numCopies = new();
        private readonly TextBox _txtDescription = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly PictureBox _picPhoto = new() { BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
        private readonly Button _btnChoosePhoto = new();
        private readonly Button _btnSave = new();
        private readonly Button _btnBack = new();
        private readonly Label _lblError = new();

        private string? _newPhotoFullPath;     // временный путь к выбранному, ещё не сохранённому файлу
        private readonly string? _currentPhotoRelativePath; // путь, уже хранящийся в БД (при редактировании)

        /// <summary>true, если книга была успешно сохранена.</summary>
        public bool Saved { get; private set; }

        public BookEditForm(bool isEdit, BookRow? existing)
        {
            _isEdit = isEdit;
            _existing = existing;
            _currentPhotoRelativePath = existing?.PhotoPath;

            InitializeComponent();
            LoadGenres();

            if (_isEdit && existing != null)
            {
                FillFromExisting(existing);
            }
            else
            {
                _txtId.Text = SafeGetNextId().ToString();
                _numCopies.Value = 1;
            }
        }

        private static int SafeGetNextId()
        {
            try
            {
                return BookService.GetNextPreviewId();
            }
            catch
            {
                return 0;
            }
        }

        private void InitializeComponent()
        {
            Text = _isEdit ? "Редактирование книги" : "Добавление книги";
            ClientSize = new Size(560, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            int labelX = 20, fieldX = 160, fieldWidth = 280, y = 20, rowHeight = 32;

            void AddRow(string label, Control control, int height = 25)
            {
                Controls.Add(new Label { Text = label, Location = new Point(labelX, y + 3), AutoSize = true });
                control.Location = new Point(fieldX, y);
                control.Size = new Size(fieldWidth, height);
                Controls.Add(control);
                y += Math.Max(rowHeight, height + 7);
            }

            AddRow("ID (авто):", _txtId);
            AddRow("Название*:", _txtTitle);
            AddRow("Автор*:", _txtAuthor);
            AddRow("Жанр*:", _cmbGenre);

            _numYear.Minimum = 0;
            _numYear.Maximum = 9999;
            _numYear.Value = DateTime.Now.Year;
            AddRow("Год издания:", _numYear);

            AddRow("Издательство:", _txtPublisher);

            _numCopies.Minimum = 0; // запрет отрицательных значений (Задание 3.3)
            _numCopies.Maximum = 100000;
            AddRow("Кол-во экземпляров*:", _numCopies);

            AddRow("Описание:", _txtDescription, 70);

            // --- Фото 300x200 ---
            Controls.Add(new Label { Text = "Фото (300×200):", Location = new Point(labelX, y + 3), AutoSize = true });
            _picPhoto.Location = new Point(fieldX, y);
            _picPhoto.Size = new Size(150, 100);
            Controls.Add(_picPhoto);

            _btnChoosePhoto.Text = "Обзор...";
            _btnChoosePhoto.Location = new Point(fieldX + 160, y + 35);
            _btnChoosePhoto.Size = new Size(100, 30);
            _btnChoosePhoto.Click += BtnChoosePhoto_Click;
            Controls.Add(_btnChoosePhoto);

            y += 115;

            _lblError.ForeColor = Color.Red;
            _lblError.Location = new Point(labelX, y);
            _lblError.Size = new Size(fieldWidth + 140, 40);
            Controls.Add(_lblError);
            y += 45;

            _btnSave.Text = _isEdit ? "Сохранить" : "Добавить";
            _btnSave.Location = new Point(fieldX, y);
            _btnSave.Size = new Size(135, 34);
            _btnSave.BackColor = Color.FromArgb(46, 139, 87);
            _btnSave.ForeColor = Color.White;
            _btnSave.FlatStyle = FlatStyle.Flat;
            _btnSave.Click += BtnSave_Click;
            Controls.Add(_btnSave);

            // Кнопка "Назад" — последовательная навигация (Задание 3.1)
            _btnBack.Text = "Назад";
            _btnBack.Location = new Point(fieldX + 145, y);
            _btnBack.Size = new Size(135, 34);
            _btnBack.FlatStyle = FlatStyle.Flat;
            _btnBack.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(_btnBack);
        }

        private void LoadGenres()
        {
            try
            {
                foreach (string genre in BookService.GetGenresForFilter())
                {
                    if (genre != BookService.AllGenresLabel)
                    {
                        _cmbGenre.Items.Add(genre);
                    }
                }
            }
            catch
            {
                // Если справочник жанров не загрузился — пользователь всё равно может ввести жанр вручную
            }
        }

        private void FillFromExisting(BookRow book)
        {
            _txtId.Text = book.Id.ToString();
            _txtTitle.Text = book.Title;
            _txtAuthor.Text = book.Author;
            _cmbGenre.Text = book.Genre;
            _numYear.Value = book.Year.HasValue ? Math.Clamp(book.Year.Value, (int)_numYear.Minimum, (int)_numYear.Maximum) : DateTime.Now.Year;
            _txtPublisher.Text = book.Publisher ?? "";
            _numCopies.Value = Math.Clamp(book.Copies, (int)_numCopies.Minimum, (int)_numCopies.Maximum);
            _txtDescription.Text = book.Description ?? "";

            if (!string.IsNullOrEmpty(book.PhotoPath))
            {
                TryShowPhoto(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, book.PhotoPath));
            }
        }

        private void BtnChoosePhoto_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Выбор фото книги",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _newPhotoFullPath = dialog.FileName;
                TryShowPhoto(_newPhotoFullPath);
            }
        }

        private void TryShowPhoto(string fullPath)
        {
            try
            {
                _picPhoto.Image?.Dispose();
                _picPhoto.Image = PhotoService.LoadForPreview(fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить изображение:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _lblError.Text = "";

            string title = _txtTitle.Text.Trim();
            string author = _txtAuthor.Text.Trim();
            string genre = _cmbGenre.Text.Trim();
            string publisher = _txtPublisher.Text.Trim();
            string description = _txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(genre))
            {
                _lblError.Text = "Заполните обязательные поля: Название, Автор, Жанр.";
                return;
            }

            try
            {
                string? photoRelativePath = _currentPhotoRelativePath;

                if (_newPhotoFullPath != null)
                {
                    // Сохраняет 300x200 копию и удаляет старое фото, если оно было (Задание 3.3)
                    photoRelativePath = PhotoService.SavePhoto(_newPhotoFullPath, _currentPhotoRelativePath);
                }

                var book = new BookRow
                {
                    Id = _isEdit ? _existing!.Id : 0,
                    Title = title,
                    Author = author,
                    Genre = genre,
                    Year = (int)_numYear.Value,
                    Publisher = string.IsNullOrEmpty(publisher) ? null : publisher,
                    Copies = (int)_numCopies.Value,
                    Description = string.IsNullOrEmpty(description) ? null : description,
                    PhotoPath = photoRelativePath
                };

                if (_isEdit)
                {
                    BookService.UpdateBook(book);
                }
                else
                {
                    BookService.InsertBook(book);
                }

                Saved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить книгу:\n{ex.Message}",
                    "Ошибка сохранения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
