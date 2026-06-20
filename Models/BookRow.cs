namespace LibraryApp.Models
{
    /// <summary>
    /// Строка каталога книг для отображения в списке (Задание 2.4).
    /// </summary>
    public class BookRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Genre { get; set; } = "";
        public int? Year { get; set; }
        public string? Publisher { get; set; }
        public int Copies { get; set; }
        public string? Description { get; set; }

        /// <summary>"Нет в наличии" / "В наличии" / "Выдана" — вычисляется по количеству экземпляров и активным выдачам.</summary>
        public string Availability { get; set; } = "";
    }
}
