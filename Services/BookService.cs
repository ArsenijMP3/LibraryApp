using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Services
{
    public enum BookSortMode
    {
        ByTitle,
        ByYearAsc,
        ByYearDesc
    }

    /// <summary>
    /// Каталог книг: получение списка (с поиском/фильтром/сортировкой — Задание 3.2)
    /// и CRUD-операции (Задание 3.3, 3.4).
    /// Наличие книги:
    ///   количество_экземпляров = 0              -> "Нет в наличии"
    ///   есть активная выдача (статус='активна')  -> "Выдана"
    ///   иначе                                    -> "В наличии"
    /// </summary>
    public static class BookService
    {
        public const string AllGenresLabel = "Все жанры";

        private const string SelectColumns = @"
            Книги.id,
            Книги.название,
            Книги.автор,
            Книги.жанр,
            Книги.год_издания,
            Книги.издательство,
            Книги.количество_экземпляров,
            Книги.описание,
            Книги.фото,
            (SELECT COUNT(*) FROM Выдачи
             WHERE Выдачи.книга_id = Книги.id AND Выдачи.статус = 'активна') AS активных_выдач";

        /// <summary>Полный список книг без фильтров (используется для гостя/читателя).</summary>
        public static List<BookRow> GetAllBooks()
        {
            string sql = $"SELECT {SelectColumns} FROM Книги ORDER BY Книги.название";
            DataTable table = DbHelper.ExecuteQuery(sql);
            return MapRows(table);
        }

        /// <summary>
        /// Поиск по названию/автору/жанру/издательству + фильтр по жанру + сортировка по году
        /// (Задание 3.2 — для библиотекаря и администратора).
        /// </summary>
        public static List<BookRow> SearchBooks(string searchText, string genreFilter, BookSortMode sortMode)
        {
            string orderBy = sortMode switch
            {
                BookSortMode.ByYearAsc => "Книги.год_издания ASC",
                BookSortMode.ByYearDesc => "Книги.год_издания DESC",
                _ => "Книги.название ASC"
            };

            string sql = $"SELECT {SelectColumns} FROM Книги WHERE " +
                          "(Книги.название LIKE ? OR Книги.автор LIKE ? OR Книги.жанр LIKE ? OR Книги.издательство LIKE ?)";

            var parameters = new List<OleDbParameter>();
            // Через параметризованный OleDbCommand движок ACE использует ANSI-маску LIKE:
            // '%' — любая последовательность символов (а не '*' как в Jet SQL из конструктора запросов Access).
            string likeText = $"%{searchText}%";
            parameters.Add(new OleDbParameter("s1", likeText));
            parameters.Add(new OleDbParameter("s2", likeText));
            parameters.Add(new OleDbParameter("s3", likeText));
            parameters.Add(new OleDbParameter("s4", likeText));

            bool hasGenreFilter = !string.IsNullOrEmpty(genreFilter) && genreFilter != AllGenresLabel;
            if (hasGenreFilter)
            {
                sql += " AND Книги.жанр = ?";
                parameters.Add(new OleDbParameter("genre", genreFilter));
            }

            sql += $" ORDER BY {orderBy}";

            DataTable table = DbHelper.ExecuteQuery(sql, parameters.ToArray());
            return MapRows(table);
        }

        /// <summary>Список уникальных жанров для выпадающего фильтра, "Все жанры" — первым элементом.</summary>
        public static List<string> GetGenresForFilter()
        {
            var genres = new List<string> { AllGenresLabel };

            DataTable table = DbHelper.ExecuteQuery(
                "SELECT DISTINCT жанр FROM Книги WHERE жанр IS NOT NULL ORDER BY жанр");

            foreach (DataRow row in table.Rows)
            {
                string? genre = row["жанр"]?.ToString();
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    genres.Add(genre);
                }
            }

            return genres;
        }

        /// <summary>Следующий "предполагаемый" id для новой записи (MAX+1) — только для отображения в форме,
        /// фактический id присваивает Access (поле AUTOINCREMENT) при вставке.</summary>
        public static int GetNextPreviewId()
        {
            object? result = DbHelper.ExecuteScalar("SELECT MAX(id) FROM Книги");
            if (result == null || result == DBNull.Value)
            {
                return 1;
            }
            return Convert.ToInt32(result) + 1;
        }

        /// <summary>Добавляет новую книгу. id присваивается автоматически (AUTOINCREMENT). Возвращает фактический id.</summary>
        public static int InsertBook(BookRow book)
        {
            const string sql = @"
                INSERT INTO Книги (название, автор, жанр, год_издания, издательство, количество_экземпляров, описание, фото)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

            return DbHelper.InsertAndGetIdentity(
                sql,
                new OleDbParameter("title", book.Title),
                new OleDbParameter("author", book.Author),
                new OleDbParameter("genre", book.Genre),
                new OleDbParameter("year", (object?)book.Year ?? DBNull.Value),
                new OleDbParameter("publisher", (object?)book.Publisher ?? DBNull.Value),
                new OleDbParameter("copies", book.Copies),
                new OleDbParameter("description", (object?)book.Description ?? DBNull.Value),
                new OleDbParameter("photo", (object?)book.PhotoPath ?? DBNull.Value));
        }

        /// <summary>Обновляет существующую запись (id неизменяем).</summary>
        public static void UpdateBook(BookRow book)
        {
            const string sql = @"
                UPDATE Книги SET
                    название = ?, автор = ?, жанр = ?, год_издания = ?,
                    издательство = ?, количество_экземпляров = ?, описание = ?, фото = ?
                WHERE id = ?";

            DbHelper.ExecuteNonQuery(
                sql,
                new OleDbParameter("title", book.Title),
                new OleDbParameter("author", book.Author),
                new OleDbParameter("genre", book.Genre),
                new OleDbParameter("year", (object?)book.Year ?? DBNull.Value),
                new OleDbParameter("publisher", (object?)book.Publisher ?? DBNull.Value),
                new OleDbParameter("copies", book.Copies),
                new OleDbParameter("description", (object?)book.Description ?? DBNull.Value),
                new OleDbParameter("photo", (object?)book.PhotoPath ?? DBNull.Value),
                new OleDbParameter("id", book.Id));
        }

        /// <summary>Количество выдач (любого статуса), связанных с книгой — для проверки перед удалением.</summary>
        public static int CountRelatedLoans(int bookId)
        {
            object? result = DbHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Выдачи WHERE книга_id = ?",
                new OleDbParameter("bookId", bookId));

            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>Удаляет книгу. Вызывающий код обязан заранее проверить CountRelatedLoans (Задание 3.4).</summary>
        public static void DeleteBook(int bookId)
        {
            DbHelper.ExecuteNonQuery(
                "DELETE FROM Книги WHERE id = ?",
                new OleDbParameter("id", bookId));
        }

        private static List<BookRow> MapRows(DataTable table)
        {
            var result = new List<BookRow>();

            foreach (DataRow row in table.Rows)
            {
                int copies = row["количество_экземпляров"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(row["количество_экземпляров"]);

                int activeIssues = Convert.ToInt32(row["активных_выдач"]);

                string availability;
                if (copies == 0)
                {
                    availability = "Нет в наличии";
                }
                else if (activeIssues > 0)
                {
                    availability = "Выдана";
                }
                else
                {
                    availability = "В наличии";
                }

                result.Add(new BookRow
                {
                    Id = Convert.ToInt32(row["id"]),
                    Title = row["название"].ToString() ?? "",
                    Author = row["автор"].ToString() ?? "",
                    Genre = row["жанр"].ToString() ?? "",
                    Year = row["год_издания"] == DBNull.Value ? null : Convert.ToInt32(row["год_издания"]),
                    Publisher = row["издательство"] == DBNull.Value ? null : row["издательство"].ToString(),
                    Copies = copies,
                    Description = row["описание"] == DBNull.Value ? null : row["описание"].ToString(),
                    PhotoPath = row["фото"] == DBNull.Value ? null : row["фото"].ToString(),
                    Availability = availability
                });
            }

            return result;
        }
    }
}
