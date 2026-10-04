using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoldsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public HoldsController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHold(Hold hold)
        {
            var user = await _context.Users.FindAsync(hold.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var book = await _context.Books.FindAsync(hold.BookId);

            if (book == null)
            {
                return NotFound("Book not found.");
            }

            if (book.IsAvailable)
            {
                return BadRequest("Cannot place a hold on an available book.");
            }
            
            _context.Holds.Add(hold);
            await _context.SaveChangesAsync();

            return Ok(hold);
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveHold(int bookId, int userId)
        {
            var hold = await _context.Holds .FirstOrDefaultAsync(h => h.BookId == bookId && h.UserId == userId);

            if (hold == null)
            {
                return NotFound("Hold not found.");
            }

            _context.Holds.Remove(hold);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
