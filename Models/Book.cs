namespace LibraryManagementSystem.Models
{
    public class Book
    {
        private static int nextId = 1000;

        public int BookId { get; private set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public bool IsAvailable { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
    }
}
