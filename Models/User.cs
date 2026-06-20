namespace LibraryApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Contact { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}