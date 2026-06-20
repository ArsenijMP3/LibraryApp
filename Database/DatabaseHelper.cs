using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using LibraryApp.Models;

namespace LibraryApp.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        
        public DatabaseHelper(string dbPath)
        {
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
        }

        public OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public User AuthenticateUser(string login, string password)
        {
            try
            {
                string query = @"
                    SELECT u.[id], u.[фио], u.[логин], u.[пароль], u.[роль_id], u.[контакт], r.[название_роли] as RoleName
                    FROM [читатель] u
                    LEFT JOIN [роль] r ON u.[роль_id] = r.[id]
                    WHERE u.[логин] = @login AND u.[пароль] = @password";

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login ?? "");
                        cmd.Parameters.AddWithValue("@password", password ?? "");

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string roleName = reader["RoleName"]?.ToString() ?? "гость";
                                
                                return new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    FullName = reader["фио"]?.ToString() ?? "Неизвестно",
                                    Login = reader["логин"]?.ToString() ?? "",
                                    Password = reader["пароль"]?.ToString() ?? "",
                                    RoleId = reader["роль_id"] != DBNull.Value ? 
                                        Convert.ToInt32(reader["роль_id"]) : 0,
                                    RoleName = roleName,
                                    Contact = reader["контакт"]?.ToString() ?? ""
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка авторизации: {ex.Message}");
                throw;
            }
            return null;
        }

        public List<Book> GetBooks()
        {
            var books = new List<Book>();
            try
            {
                string query = @"
                    SELECT 
                        [id], 
                        [название], 
                        [автор], 
                        [жанр], 
                        [год_издания], 
                        [издательство], 
                        [количество_экземпляров], 
                        [описание], 
                        [фото]
                    FROM [книга]
                    ORDER BY [название]";

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new OleDbCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0;
                                string title = reader["название"]?.ToString() ?? "Без названия";
                                string author = reader["автор"]?.ToString() ?? "Неизвестен";
                                string genre = reader["жанр"]?.ToString() ?? "Не указан";
                                int year = reader["год_издания"] != DBNull.Value ? 
                                    Convert.ToInt32(reader["год_издания"]) : 0;
                                string publisher = reader["издательство"]?.ToString() ?? "Не указано";
                                int copies = reader["количество_экземпляров"] != DBNull.Value ? 
                                    Convert.ToInt32(reader["количество_экземпляров"]) : 0;
                                string description = reader["описание"]?.ToString() ?? "";
                                string photo = reader["фото"]?.ToString() ?? "";

                                bool isIssued = IsBookIssued(id);

                                var book = new Book
                                {
                                    Id = id,
                                    Title = title,
                                    Author = author,
                                    Genre = genre,
                                    Year = year,
                                    Publisher = publisher,
                                    Copies = copies,
                                    Description = description,
                                    PhotoPath = photo
                                };
                                
                                if (copies == 0)
                                {
                                    book.Status = "Нет в наличии";
                                }
                                else if (isIssued)
                                {
                                    book.Status = "Выдана";
                                }
                                else
                                {
                                    book.Status = "В наличии";
                                }
                                
                                books.Add(book);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения книг: {ex.Message}");
                throw;
            }
            return books;
        }

        public bool IsBookIssued(int bookId)
        {
            if (bookId <= 0) return false;
            
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM [выдача] 
                    WHERE [книга_id] = @bookId AND LOWER([статус]) = 'активна'";

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@bookId", bookId);
                        var result = cmd.ExecuteScalar();
                        return result != null && Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsAdmin(User user)
        {
            if (user == null) return false;
            string role = user.RoleName?.ToLower() ?? "";
            return role == "администратор";
        }

        public bool IsLibrarian(User user)
        {
            if (user == null) return false;
            string role = user.RoleName?.ToLower() ?? "";
            return role == "библиотекарь";
        }

        public bool CanViewIssues(User user)
        {
            if (user == null) return false;
            string role = user.RoleName?.ToLower() ?? "";
            return role == "библиотекарь" || role == "администратор";
        }
    }
}