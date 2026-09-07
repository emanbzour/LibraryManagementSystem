using LibraryManagementSystem.Models;
using Serilog.Events;
using System.Threading;

namespace LibraryManagementSystem.Services
{
    public class LibraryService
    {
        private List<User> users = new();
        private List<Book> books = new();
        private List<Transaction> transactions = new();
        private List<Hold> holds = new();
        private readonly object lockObject = new object();
        private DateTime lastActivityTime = DateTime.Now;
        private FileStorageService fileStorageService = new FileStorageService();
        private LogService logService = new LogService();
        private Timer? autoSaveTimer;
        private Timer? backupTimer;
        private Timer? cleanupTimer;
        private Timer? idleTimer;
        private bool hasUnsavedChanges = false;
        public bool HasUnsavedChanges => hasUnsavedChanges;

        public LibraryService()
        {
            autoSaveTimer = new Timer(AutoSave, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
            backupTimer = new Timer(DailyBackup, null, DateTime.Today.AddDays(1) - DateTime.Now, TimeSpan.FromDays(1));
            cleanupTimer = new Timer(DeleteOldLogs, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            idleTimer = new Timer(CheckIdle, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        // Automatically saves library data every two minutes.
        private void AutoSave(object? state)
        {
            SaveData();
            logService.Log("Data saved successfully.", LogEventLevel.Information);
        }

        // Creates a daily backup of library data.
        private void DailyBackup(object? state)
        {
            bool result = fileStorageService.BackupData(users, books, transactions, holds);
            if (result) logService.Log("Daily backup completed successfully.", LogEventLevel.Information);
        }

        /// <summary>
        /// Saves all library data to files.
        /// </summary>
        /// <returns>True if the data is saved successfully.</returns>
        public bool SaveData()
        {
            bool result = fileStorageService.SaveData(users, books, transactions, holds);
            if (result)
            {
                hasUnsavedChanges = false;
                logService.Log("Data saved successfully.", LogEventLevel.Information);
            }
            return result;
        }

        /// <summary>
        /// Registers a new library member.
        /// </summary>
        /// <param name="userName">The name of the member.</param>
        /// <param name="phoneNumber">The phone number of the member.</param>
        /// <param name="address">The address of the member.</param>
        /// <returns>True if the member is registered successfully.</returns>
        public bool RegisterNewMember(string userName, string phoneNumber, string address)
        {
            lock (lockObject)
            {
                User user = new User(userName, phoneNumber, address);
                users.Add(user);

                logService.Log($"New member registered: UserId={user.UserId}, Name={user.UserName}", LogEventLevel.Information);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;
                return true;
            }
        }

        /// <summary>
        /// Loads all saved library data from files.
        /// </summary>
        /// <returns>True if the data is loaded successfully.</returns>
        public bool LoadData()
        {
            var data = fileStorageService.LoadData();

            users = data.users;
            books = data.books;
            transactions = data.transactions;
            holds = data.holds;
            logService.Log("Data loaded successfully.", LogEventLevel.Information);
            return true;
        }

        /// <summary>
        /// Adds a new book to the library.
        /// </summary>
        /// <param name="title">The title of the book.</param>
        /// <param name="author">The author of the book.</param>
        /// <returns>True if the book is added successfully.</returns>
        public bool AddBook(string title, string author)
        {
            lock (lockObject)
            {
                Book book = new Book(title, author);
                books.Add(book);
                logService.Log($"New book added: BookId={book.BookId}, Title={book.Title}", LogEventLevel.Information);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;
                return true;
            }
        }

        /// <summary>
        /// Removes a book from the library.
        /// </summary>
        /// <param name="bookId">The ID of the book to remove.</param>
        /// <returns>True if the book is removed successfully.</returns>
        public bool RemoveBook(int bookId)
        {
            lock (lockObject)
            {
                Book? book = books.FirstOrDefault(b => b.BookId == bookId);

                if (book == null)
                {
                    return false;
                }

                books.Remove(book);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;
                logService.Log($" Book removed: BookId={book.BookId}, Title={book.Title}", LogEventLevel.Information);
                return true;
            }
        }

        /// <summary>
        /// Checks out one or more books to a library member.
        /// </summary>
        /// <param name="userId">The ID of the member.</param>
        /// <param name="bookIds">The IDs of the books to check out.</param>
        /// <returns>True if the checkout is successful.</returns>
        public bool CheckoutBook(int userId, List<int> bookIds)
        {
            User? user = users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return false;
            }

            lock (lockObject)
            {
                List<Book> selectedBooks = new();

                foreach (int bookId in bookIds)
                {
                    Book? book = books.FirstOrDefault(b => b.BookId == bookId);

                    if (book == null)
                    {
                        Console.WriteLine($"Book {bookId} does not exist.");
                        continue;
                    }

                    if (!book.IsAvailable)
                    {
                        Console.WriteLine($"Book {bookId} is not available.");
                        continue;
                    }

                    selectedBooks.Add(book);
                }

                if (selectedBooks.Count == 0)
                {
                    return false;
                }

                List<int> availableBookIds = selectedBooks
                    .Select(book => book.BookId)
                    .ToList();

                Transaction transaction = new(userId, availableBookIds);

                transactions.Add(transaction);

                foreach (Book book in selectedBooks)
                {
                    book.IsAvailable = false;
                }

                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;
                logService.Log($"Book checkout: UserId={userId}, BookIds={string.Join(", ", availableBookIds)}", LogEventLevel.Information);
                return true;
            }
        }

        /// <summary>
        /// Records the return of a checked-out book.
        /// </summary>
        /// <param name="bookId">The ID of the returned book.</param>
        /// <returns>True if the book is returned successfully.</returns>
        public bool ReturnBook(int bookId)
        {
            lock (lockObject)
            {
                Book? book = books.FirstOrDefault(b => b.BookId == bookId);

                if (book == null)
                {
                    return false;
                }

                Transaction? transaction = transactions.FirstOrDefault(t => t.BookIds.Contains(bookId) && t.TransactionType == "Checkout");

                if (transaction == null)
                {
                    return false;
                }

                int userId = transaction.UserId;

                book.IsAvailable = true;
                book.ReturnDate = DateTime.Now;

                Transaction returnTransaction = new(userId, new List<int> { bookId });
                returnTransaction.TransactionType = "Return";
                transactions.Add(returnTransaction);

                if (holds.Any(h => h.BookId == bookId))
                {
                    Console.WriteLine("This book has an existing hold. Set it aside for the reserved user.");
                }

                logService.Log($"Book returned: BookId={book.BookId}, UserId={userId}", LogEventLevel.Information);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;

                return true;
            }
        }

        /// <summary>
        /// Places a hold on an unavailable book for a member.
        /// </summary>
        /// <param name="bookId">The ID of the book.</param>
        /// <param name="userId">The ID of the member.</param>
        /// <param name="startDate">The start date of the period.</param>
        /// <param name="endDate">The end date of the period.</param>
        /// <returns>True if the hold is placed successfully.</returns>
        public bool PlaceHold(int bookId, int userId, DateTime startDate, DateTime endDate)
        {
            lock (lockObject)
            {
                User? user = users.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return false;
                }

                Book? book = books.FirstOrDefault(b => b.BookId == bookId);

                if (book == null)
                {
                    return false;
                }

                if (book.IsAvailable)
                {
                    return false;
                }

                Hold hold = new(userId, bookId, startDate, endDate);

                holds.Add(hold);
                logService.Log($"Hold placed: UserId={userId}, BookId={bookId}", LogEventLevel.Information);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;

                return true;
            }
        }

        /// <summary>
        /// Removes a hold placed by a member.
        /// </summary>
        /// <param name="userId">The ID of the member.</param>
        /// <param name="bookId">The ID of the book.</param>
        /// <returns>True if the hold is removed successfully.</returns>
        public bool RemoveHold(int userId, int bookId)
        {
            lock (lockObject)
            {
                Hold? hold = holds.FirstOrDefault(h => h.UserId == userId && h.BookId == bookId);

                if (hold == null)
                {
                    return false;
                }

                holds.Remove(hold);
                logService.Log($"Hold removed: UserId={userId}, BookId={bookId}", LogEventLevel.Information);
                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;
                return true;
            }
        }

        /// <summary>
        /// Renews a book for a library member.
        /// </summary>
        /// <param name="userId">The ID of the member.</param>
        /// <returns>True if the book is renewed successfully.</returns>
        public bool RenewBook(int userId)
        {
            lock (lockObject)
            {
                List<Transaction> transactionsForUser = transactions
                    .Where(t => t.UserId == userId && t.TransactionType == "Checkout")
                    .ToList();

                List<int> userBookIds = transactionsForUser
                    .SelectMany(t => t.BookIds)
                    .ToList();

                List<Book> booksForUser = books
                    .Where(book => userBookIds.Contains(book.BookId) && !book.IsAvailable)
                    .ToList();

                if (booksForUser.Count == 0)
                {
                    return false;
                }

                foreach (Book b in booksForUser)
                {
                    Console.WriteLine($"Book name: {b.Title}\tBook ID: {b.BookId}");
                }

                Console.WriteLine("Enter the book ID to renew:");

                int bookIdToRenew = Convert.ToInt32(Console.ReadLine());

                Book? book = booksForUser.FirstOrDefault(b => b.BookId == bookIdToRenew);

                if (book == null)
                {
                    Console.WriteLine("Invalid ID number. Try again.");
                    return false;
                }

                Transaction? transaction = transactionsForUser.FirstOrDefault(t => t.BookIds.Contains(bookIdToRenew));

                if (transaction == null)
                {
                    return false;
                }

                transaction.DueDate = transaction.DueDate.AddDays(7);

                Transaction renewTransaction = new(userId, new List<int> { bookIdToRenew });
                renewTransaction.TransactionType = "Renew";
                renewTransaction.DueDate = transaction.DueDate;
                transactions.Add(renewTransaction);

                logService.Log($"Book renewed: UserId={userId}, BookId={bookIdToRenew}", LogEventLevel.Information);

                Console.WriteLine($"Book name: {book.Title}\tBook ID: {book.BookId} was renewed successfully.");

                lastActivityTime = DateTime.Now;
                hasUnsavedChanges = true;

                return true;
            }
        }

        /// <summary>
        /// Prints a member's transactions within a specified date range.
        /// </summary>
        /// <param name="userId">The ID of the member.</param>
        /// <param name="fromDate">The start date of the transaction range.</param>
        /// <param name="toDate">The end date of the transaction range.</param>
        /// <returns>True if transactions are printed successfully.</returns>
        public bool PrintUserTransactions(int userId, DateTime fromDate, DateTime? toDate = null)
        {
            User? user = users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return false;
            }

            List<Transaction> userTransactions = transactions
                .Where(t => t.UserId == userId)
                .ToList();

            if (userTransactions.Count == 0)
            {
                return false;
            }

            DateTime endDate = toDate ?? fromDate;

            List<Transaction> filteredTransactions = userTransactions
                .Where(t => t.CheckoutDate.Date >= fromDate.Date &&
                            t.CheckoutDate.Date <= endDate.Date)
                .ToList();

            if (filteredTransactions.Count == 0)
            {
                return false;
            }

            foreach (Transaction transaction in filteredTransactions)
            {
                Console.WriteLine($"User ID: {transaction.UserId}");
                Console.WriteLine($"Transaction Type: {transaction.TransactionType}");
                Console.WriteLine($"Transaction Date: {transaction.CheckoutDate}");

                if (transaction.TransactionType != "Return")
                {
                    Console.WriteLine($"Due Date: {transaction.DueDate}");
                }

                Console.WriteLine("Books:");

                foreach (int bookId in transaction.BookIds)
                {
                    Book? book = books.FirstOrDefault(b => b.BookId == bookId);

                    if (book != null)
                    {
                        Console.WriteLine($"Book ID: {book.BookId}\tBook Name: {book.Title}");
                    }
                }

                Console.WriteLine("-------------------------");
            }

            logService.Log($"Transactions printed: UserId={userId}, From={fromDate:yyyy-MM-dd}, To={endDate:yyyy-MM-dd}", LogEventLevel.Information);
            lastActivityTime = DateTime.Now;

            return true;
        }

        /// <summary>
        /// Notifies members when their held books become available.
        /// </summary>
        /// <returns>True if notifications are processed successfully.</returns>
        public bool NotifyMembersOfBookAvailability()
        {
            List<Book> BookToday = books
                .Where(b => b.ReturnDate.HasValue &&
                            b.ReturnDate.Value.Date == DateTime.Now.Date)
                .ToList();

            if (BookToday.Count == 0)
            {
                return false;
            }

            foreach (Book book in BookToday)
            {
                List<Hold> bookHolds = holds
                    .Where(h => h.BookId == book.BookId)
                    .ToList();

                if (bookHolds.Count == 0)
                {
                    continue;
                }

                foreach (Hold hold in bookHolds)
                {
                    User? holduser = users
                        .FirstOrDefault(u => u.UserId == hold.UserId);

                    if (holduser == null)
                    {
                        continue;
                    }

                    Console.WriteLine(
                        $"Notification: {holduser.UserName}, " +
                        $"Book '{book.Title}' is now available. " +
                        $"Phone: {holduser.PhoneNumber}");
                }
            }

            logService.Log($"Book availability notifications sent: ReturnedBooks={BookToday.Count}", LogEventLevel.Information);
            lastActivityTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Searches for books by title.
        /// </summary>
        /// <param name="searchText">The text to search for in book titles.</param>
        /// <returns>A list of books matching the search text.</returns>
        public List<Book> SearchBooks(string searchText)
        {
            List<Book> suggestions = books.Where(b => b.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            return suggestions;
        }

        /// <summary>
        /// Displays book suggestions while the user types a search.
        /// </summary>
        public void SearchSuggestions()
        {
            string searchText = "";

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }

                searchText += key.KeyChar;

                List<Book> suggestions = SearchBooksInBackground(searchText).Result;

                Console.Clear();

                Console.WriteLine($"Search: {searchText}");
                Console.WriteLine("Suggestions:");

                foreach (Book book in suggestions) Console.WriteLine(book.Title);
            }
        }

        // Runs book search in the background.
        public Task<List<Book>> SearchBooksInBackground(string searchText)
        {
            return Task.Run(() => SearchBooks(searchText));
        }

        // Deletes log files older than 30 days automatically.
        public void DeleteOldLogs(object? state)
        {
            try
            {
                string logDirectory = "Logs";

                foreach (string filePath in Directory.GetFiles(logDirectory))
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                    DateTime cutoffDate = DateTime.Now.AddDays(-30);

                    if (lastWriteTime < cutoffDate)
                    {
                        File.Delete(filePath);
                        logService.Log($"Old log deleted: {filePath}", LogEventLevel.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                logService.Log($"Error deleting old logs: {ex.Message}", LogEventLevel.Error);
            }
        }

        // Checks whether the user has been idle for five minutes and reminds them about unsaved changes.
        private void CheckIdle(object? state)
        {
            TimeSpan dif = DateTime.Now - lastActivityTime;

            if (dif >= TimeSpan.FromMinutes(5) && hasUnsavedChanges)
            {
                Console.WriteLine("You have unsaved changes. Please save your data.");
            }
        }
    }
}

