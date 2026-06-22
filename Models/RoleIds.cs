namespace LibraryApp.Models
{
    /// <summary>
    /// Id ролей — соответствуют записям таблицы Роли в базе данных.
    /// Гость (0) — условная роль для входа без аккаунта, в таблице Роли её id = 1,
    /// но т.к. гость не проходит авторизацию по логину, в приложении используем 0.
    /// </summary>
    public static class RoleIds
    {
        public const int Guest = 0;
        public const int Reader = 2;
        public const int Librarian = 3;
        public const int Admin = 4;
    }
}
