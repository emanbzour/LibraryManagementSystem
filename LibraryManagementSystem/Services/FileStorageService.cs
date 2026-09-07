using System;
using System.Collections.Generic;
using System.Text.Json;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class FileStorageService
    {
        public bool SaveData(
            List<User> users,
            List<Book> books,
            List<Transaction> transactions,
            List<Hold> holds)
        {
            bool usersSaved = SaveToFile(users, "users.json");
            bool booksSaved = SaveToFile(books, "books.json");
            bool transactionsSaved = SaveToFile(transactions, "transactions.json");
            bool holdsSaved = SaveToFile(holds, "holds.json");

            return usersSaved &&
                   booksSaved &&
                   transactionsSaved &&
                   holdsSaved;
        }

        public (
            List<User> users,
            List<Book> books,
            List<Transaction> transactions,
            List<Hold> holds
        ) LoadData()
        {
            List<User> users = LoadFromFile<User>("users.json");
            List<Book> books = LoadFromFile<Book>("books.json");
            List<Transaction> transactions = LoadFromFile<Transaction>("transactions.json");
            List<Hold> holds = LoadFromFile<Hold>("holds.json");

            return (users, books, transactions, holds);
        }

        public List<T> LoadFromFile<T>(string fileName)
        {
            try
            {
                string json = File.ReadAllText(fileName);

                List<T>? list = JsonSerializer.Deserialize<List<T>>(json);

                return list ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {fileName}: {ex.Message}");
                return new List<T>();
            }
        }

        public bool SaveToFile<T>(List<T> list, string fileName)
        {
            try
            {
                string json = JsonSerializer.Serialize(list);
                File.WriteAllText(fileName, json);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {fileName}: {ex.Message}");
                return false;
            }
        }
        public bool BackupData(List<User> users, List<Book> books, List<Transaction> transactions, List<Hold> holds)
        {
            Directory.CreateDirectory("Backups");
            string backupFileName = $"Backups/backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            string json = JsonSerializer.Serialize(new { users, books, transactions, holds });
            File.WriteAllText(backupFileName, json);
            return true;
        }
    }
}