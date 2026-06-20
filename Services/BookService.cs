using System;
using System.Collections.Generic;
using System.Data;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Services
{
    /// <summary>
    /// Получение каталога книг с вычислением наличия (Задание 2.4).
    /// Наличие книги:
    ///   количество_экземпляров = 0           -> "Нет в наличии"
    ///   есть активная выдача (статус='активна') -> "Выдана"
    ///   иначе                                 -> "В наличии"
    /// </summary>
    public static class BookService
    {
        public static List<BookRow> GetAllBooks()
        {
            const string sql = @"
                SELECT
                    Книги.id,
                    Книги.название,
                    Книги.автор,
                    Книги.жанр,
                    Книги.год_издания,
                    Книги.издательство,
                    Книги.количество_экземпляров,
                    Книги.описание,
                    (SELECT COUNT(*) FROM Выдачи
                     WHERE Выдачи.книга_id = Книги.id AND Выдачи.статус = 'активна') AS активных_выдач
                FROM Книги
                ORDER BY Книги.название";

            DataTable table = DbHelper.ExecuteQuery(sql);
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
                    Availability = availability
                });
            }

            return result;
        }
    }
}
