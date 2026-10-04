namespace LibraryManagementSystem.Models
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public List<int> BookIds { get; set; }
    }
}