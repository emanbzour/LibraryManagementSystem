using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagementSystem.Models
{
    public class Hold(int userId , int bookId , DateTime startDate , DateTime endDate)
    {
        public int UserId { get; set; } = userId;
        public int BookId { get; set; } = bookId;
        public DateTime StartDate { get; set; } = startDate;
        public DateTime EndDate { get; set; } = endDate;


    }
}
