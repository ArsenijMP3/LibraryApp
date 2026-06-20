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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "base.accdb");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Файл базы данных не найден: {path}\n" +
                    "Скопируйте base.accdb в папку с .exe (или рядом с проектом, " +
                    "тогда он скопируется автоматически при сборке).");
            }

            return path;
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
    }
}
