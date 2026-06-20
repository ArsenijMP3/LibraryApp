namespace LibraryApp.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public int Copies { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}