using System;
using System.Data;
using System.Data.OleDb;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Services
{
    /// <summary>
    /// Проверка логина/пароля по таблицам Читатели + Роли (Задание 2.3).
    /// </summary>
    public static class AuthService
    {
        /// <summary>Возвращает данные пользователя, если логин/пароль верны, иначе null.</summary>
        public static Reader? TryLogin(string login, string password)
        {
            const string sql = @"
                SELECT Читатели.id, Читатели.фио, Читатели.логин,
                       Читатели.роль_id, Роли.название_роли, Читатели.контакт
                FROM Читатели INNER JOIN Роли ON Читатели.роль_id = Роли.id
                WHERE Читатели.логин = ? AND Читатели.пароль = ?";

            DataTable table = DbHelper.ExecuteQuery(
                sql,
                new OleDbParameter("login", login),
                new OleDbParameter("password", password));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];

            return new Reader
            {
                Id = Convert.ToInt32(row["id"]),
                Fio = row["фио"].ToString() ?? "",
                Login = row["логин"].ToString() ?? "",
                RoleId = Convert.ToInt32(row["роль_id"]),
                RoleName = row["название_роли"].ToString() ?? "",
                Contact = row["контакт"] == DBNull.Value ? null : row["контакт"].ToString()
            };
        }
    }
}
