using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public TransactionsController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
                return NotFound("User not found.");

            var books = await _context.Books
                .Where(b => request.BookIds.Contains(b.BookId))
                .ToListAsync();

            if (books.Count != request.BookIds.Count)
                return NotFound("One or more books not found.");

            if (books.Any(b => !b.IsAvailable))
                return BadRequest("One or more books are not available.");

            foreach (var book in books)
            {
                book.IsAvailable = false;
            }

            var transaction = new Transaction
            {
                UserId = request.UserId,
                TransactionType = TransactionType.Checkout,
                Date = DateTime.Now,
                Books = books
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook(ReturnRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
                return NotFound("User not found.");

            var book = await _context.Books.FindAsync(request.BookId);

            if (book == null)
                return NotFound("Book not found.");

            if (book.IsAvailable)
                return BadRequest("Book is already available.");

            book.IsAvailable = true;

            var transaction = new Transaction
            {
                UserId = request.UserId,
                TransactionType = TransactionType.Return,
                Date = DateTime.Now,
                Books = new List<Book> { book }
            };

            _context.Transactions.Add(transaction);

            var holdExists = await _context.Holds
                .AnyAsync(h => h.BookId == request.BookId);

            await _context.SaveChangesAsync();

            if (holdExists)
            {
                return Ok(new
                {
                    message = "Book returned. There is an existing hold. Alert the clerk to set it aside.",
                    transaction
                });
            }

            return Ok(transaction);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserTransactions(
            [FromRoute] int id,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == id)
                .Include(t => t.Books)
                .ToListAsync();

            if (fromDate.HasValue)
            {
                transactions = transactions
                    .Where(t => t.Date >= fromDate.Value)
                    .ToList();
            }

            if (toDate.HasValue)
            {
                transactions = transactions
                    .Where(t => t.Date < toDate.Value.Date.AddDays(1))
                    .ToList();
            }

            return Ok(transactions);
        }

        [HttpGet("user/{id}/books")]
        public async Task<IActionResult> GetBorrowedBooks(int id)
        {
            var transactions = await _context.Transactions
                .Where(t =>
                    t.UserId == id &&
                    (t.TransactionType == TransactionType.Checkout ||
                     t.TransactionType == TransactionType.Renew))
                .Include(t => t.Books)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.Books = transaction.Books
                    .Where(b => !b.IsAvailable)
                    .ToList();
            }

            transactions = transactions
                .Where(t => t.Books.Any())
                .ToList();

            return Ok(transactions);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> RenewBook(RenewRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var book = await _context.Books.FindAsync(request.BookId);

            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.IsAvailable)
            {
                return BadRequest("This book is not currently borrowed.");
            }

            var isBorrowed = await _context.Transactions
                .Where(t =>
                    t.UserId == request.UserId &&
                    (t.TransactionType == TransactionType.Checkout ||
                     t.TransactionType == TransactionType.Renew))
                .AnyAsync(t => t.Books.Any(b => b.BookId == request.BookId));

            if (!isBorrowed)
            {
                return BadRequest("This book is not borrowed by this user.");
            }

            var transaction = new Transaction
            {
                UserId = request.UserId,
                TransactionType = TransactionType.Renew,
                Date = DateTime.Now,
                Books = new List<Book> { book }
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Book renewed successfully.",
                transaction
            });
        }

        [HttpGet("availability-notifications")]
        public async Task<IActionResult> GetAvailabilityNotifications()
        {
            var today = DateTime.Today;

            var returnedTransactions = await _context.Transactions
                .Where(t =>
                    t.TransactionType == TransactionType.Return &&
                    t.Date >= today &&
                    t.Date < today.AddDays(1))
                .Include(t => t.Books)
                .ToListAsync();

            var returnedBookIds = returnedTransactions
                .SelectMany(t => t.Books)
                .Select(b => b.BookId)
                .ToList();

            var holds = await _context.Holds
                .Where(h => returnedBookIds.Contains(h.BookId))
                .Include(h => h.User)
                .Include(h => h.Book)
                .ToListAsync();

            return Ok(holds.Select(h => new
            {
                BookId = h.BookId,
                UserId = h.UserId,
                UserName = h.User.UserName,
                PhoneNumber = h.User.PhoneNumber
            }));
        }
    }
}
