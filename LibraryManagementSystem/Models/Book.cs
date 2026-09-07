using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagementSystem.Models
{
    public class Book (string title , string author )
    {
        private static int nextId=1000;
        public int BookId { get; private set; } = nextId++;
        public string Title { get; set; } = title;
        public string Author { get; set; } = author;
        public bool IsAvailable { get; set; } = true;
        public DateTime? ReturnDate { get; set; }
    }
}
