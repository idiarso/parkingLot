using System;
using System.Threading.Tasks;

namespace TestWpfApp
{
    class DatabaseTest
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Database Connection Test ===\n");
            
            // Test database connection
            Console.WriteLine("Testing database connection...");
            string connectionResult = await DatabaseCheck.CheckDatabaseConnection();
            Console.WriteLine(connectionResult);
            Console.WriteLine();
            
            // Check users table
            Console.WriteLine("Checking users table...");
            string usersTableResult = await DatabaseCheck.CheckUsersTable();
            Console.WriteLine(usersTableResult);
            Console.WriteLine();
            
            // Create admin user
            Console.WriteLine("Creating/resetting admin user...");
            string adminCreationResult = await DatabaseCheck.CreateAdminUser();
            Console.WriteLine(adminCreationResult);
            Console.WriteLine();
            
            // Verify admin user was created
            Console.WriteLine("Verifying admin user...");
            usersTableResult = await DatabaseCheck.CheckUsersTable();
            Console.WriteLine(usersTableResult);
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
