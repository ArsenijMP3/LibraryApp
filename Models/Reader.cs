namespace LibraryApp.Models
{
    /// <summary>
    /// Авторизованный пользователь (запись из таблицы Читатели + название роли из Роли).
    /// </summary>
    public class Reader
    {
        public int Id { get; set; }
        public string Fio { get; set; } = "";
        public string Login { get; set; } = "";
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public string? Contact { get; set; }
    }
}
