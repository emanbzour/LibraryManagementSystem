using System;

namespace LibraryManagementSystem.Models
{
    public class User(string userName, string phoneNumber, string address)
    {
        private static int nextId = 1000;

        public int UserId { get; private set; } = nextId++;
        public string UserName { get; set; } = userName;
        public string PhoneNumber { get; set; } = phoneNumber;
        public string Address { get; set; } = address;
    }
}