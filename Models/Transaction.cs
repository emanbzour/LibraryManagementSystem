namespace LibraryManagementSystem.Models
{
    public class Transaction
    {

        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
        public List<Book> Books { get; set; } = new();
    }
}
