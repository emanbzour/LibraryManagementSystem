using LibraryManagementSystem.Services;

namespace LibraryManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LibraryService libraryService = new LibraryService();

            libraryService.LoadData();
           

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            if (libraryService.HasUnsavedChanges)
            {
                Console.WriteLine("You have unsaved changes. Do you want to save before exiting? (y/n)");
                string? answer = Console.ReadLine();

                if (answer?.ToLower() == "y")
                {
                    libraryService.SaveData();
                }
            }

            
        }
    }
}
