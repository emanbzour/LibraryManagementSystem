namespace LibraryManagementSystem.Models
{
    public class User
    {
        private static int nextId = 1000;

        public int UserId { get; private set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
