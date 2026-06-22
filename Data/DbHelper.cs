using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace LibraryApp.Data
{
    /// <summary>
    /// Подключение к base.accdb через OLE DB (Microsoft.ACE.OLEDB.12.0).
    /// Требует установленного Access Database Engine (если Access/Office не стоит —
    /// поставить "Microsoft Access Database Engine 2016 Redistributable").
    /// </summary>
    public static class DbHelper
    {
        private static string GetDbPath()
        {
            string projectDb = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "base.accdb"));

            if (File.Exists(projectDb))
                return projectDb;

            string localDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "base.accdb");

            if (File.Exists(localDb))
                return localDb;

            throw new FileNotFoundException($"Не найден файл БД: {projectDb} или {localDb}");
        }

        public static string ConnectionString =>
            $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GetDbPath()};Persist Security Info=False;";

        public static OleDbConnection GetConnection() => new OleDbConnection(ConnectionString);

        /// <summary>Выполняет SELECT и возвращает результат в виде DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, params OleDbParameter[] parameters)
        {
            var table = new DataTable();

            using var connection = GetConnection();
            using var command = new OleDbCommand(sql, connection);

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();

            using var adapter = new OleDbDataAdapter(command);
            adapter.Fill(table);

            return table;
        }

        /// <summary>Выполняет INSERT/UPDATE/DELETE, возвращает число затронутых строк.</summary>
        public static int ExecuteNonQuery(string sql, params OleDbParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new OleDbCommand(sql, connection);

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            return command.ExecuteNonQuery();
        }

        /// <summary>Выполняет SELECT, возвращающий одно значение (например, COUNT(*) или MAX(id)).</summary>
        public static object? ExecuteScalar(string sql, params OleDbParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new OleDbCommand(sql, connection);

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Выполняет INSERT и возвращает значение AUTOINCREMENT-поля только что добавленной записи
        /// (через @@IDENTITY на той же открытой соединении — обязательно в рамках одной сессии).
        /// </summary>
        public static int InsertAndGetIdentity(string sql, params OleDbParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = new OleDbCommand(sql, connection);

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            command.ExecuteNonQuery();

            using var identityCommand = new OleDbCommand("SELECT @@IDENTITY", connection);
            object? result = identityCommand.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
    }
}
