using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagementSystem.Models
{
    public class Transaction(int userId, List<int> bookIds)
    {
        public int UserId { get; set; } = userId;
        public List<int> BookIds { get; set; } = bookIds;
        public DateTime CheckoutDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public string TransactionType { get; set; } = "Checkout";
    }
}