using System;
namespace LibraryManagementSystem.Models
{
    public class Hold
    {
        public int HoldId { get; set; }

        public int UserId { get; set; }
        public int BookId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public User? User { get; set; }
        public Book? Book { get; set; }
    }
}
