using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using LibraryApp.Data;

namespace LibraryApp.Forms
{
    /// <summary>
    /// Просмотр всех выдач — для библиотекаря и администратора.
    /// Навигация: открывается кнопкой «Выдачи» из главного окна, кнопка «Назад» закрывает (Задание 3.1).
    /// </summary>
    public class LoansForm : Form
    {
        private readonly DataGridView _grid = new();

        public LoansForm()
        {
            InitializeComponent();
            LoadLoans();
        }

        private void InitializeComponent()
        {
            Text = "Библиотека — Выдачи";
            ClientSize = new Size(1000, 560);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(800, 400);
            Font = new Font("Segoe UI", 9.5F);

            // --- верхняя панель ---
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(33, 47, 61)
            };

            var lblTitle = new Label
            {
                Text = "Список выдач",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true
            };
            lblTitle.Location = new Point(15, (topPanel.Height - lblTitle.PreferredHeight) / 2);

            var btnBack = new Button
            {
                Text = "← Назад",
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };
            btnBack.Click += (_, _) => Close();
            topPanel.Resize += (_, _) =>
            {
                btnBack.Location = new Point(
                    topPanel.Width - btnBack.Width - 15,
                    (topPanel.Height - btnBack.Height) / 2);
            };

            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(btnBack);

            // --- таблица выдач ---
            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.MultiSelect = false;

            Controls.Add(_grid);
            Controls.Add(topPanel);
        }

        private void LoadLoans()
        {
            try
            {
                const string sql = @"
                    SELECT
                        Выдачи.id                                           AS [№],
                        Книги.название                                      AS [Книга],
                        Книги.автор                                         AS [Автор],
                        Читатели.фио                                        AS [Читатель],
                        Выдачи.дата_выдачи                                  AS [Дата выдачи],
                        IIf(Выдачи.дата_возврата IS NULL,
                            '—', CStr(Выдачи.дата_возврата))               AS [Дата возврата],
                        Выдачи.статус                                       AS [Статус]
                    FROM (Выдачи
                        INNER JOIN Книги    ON Выдачи.книга_id    = Книги.id)
                        INNER JOIN Читатели ON Выдачи.читатель_id = Читатели.id
                    ORDER BY Выдачи.дата_выдачи DESC";

                DataTable table = DbHelper.ExecuteQuery(sql);
                _grid.DataSource = table;

                // Раскраска по статусу
                _grid.RowPrePaint += (_, e) =>
                {
                    if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
                    string status = _grid.Rows[e.RowIndex].Cells["Статус"].Value?.ToString() ?? "";
                    _grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = status switch
                    {
                        "активна"    => Color.FromArgb(173, 216, 230),   // голубой
                        "возвращена" => Color.FromArgb(46,  139,  87),   // зелёный
                        "просрочена" => Color.FromArgb(220,  53,  69),   // красный
                        _            => Color.White
                    };
                    _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor =
                        status == "возвращена" || status == "просрочена" ? Color.White : Color.Black;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список выдач:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
