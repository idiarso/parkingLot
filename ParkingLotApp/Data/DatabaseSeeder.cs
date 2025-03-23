using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using System.Collections.Generic;

namespace ParkingLotApp.Data
{
    public class DatabaseSeeder
    {
        private readonly ParkingDbContext _dbContext;
        
        public DatabaseSeeder(ParkingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task SeedAsync()
        {
            try
            {
                Console.WriteLine("[Info] Starting database seeding process...");
                
                // Ensure database exists and has applied migrations
                Console.WriteLine("[Info] Ensuring database exists and schema is up to date...");
                await EnsureDatabaseCreatedAsync();
                
                // Seed roles if they don't exist
                Console.WriteLine("[Info] Seeding roles...");
                await SeedRolesAsync();
                
                // Seed admin user if no users exist
                Console.WriteLine("[Info] Seeding admin user...");
                await SeedAdminUserAsync();
                
                // Seed default settings
                Console.WriteLine("[Info] Seeding default settings...");
                await SeedDefaultSettingsAsync();
                
                // Seed sample parking activities
                Console.WriteLine("[Info] Seeding sample parking activities...");
                await SeedSampleParkingActivitiesAsync();
                
                // Seed sample logs
                Console.WriteLine("[Info] Seeding sample logs...");
                await SeedSampleLogsAsync();
                
                Console.WriteLine("[Info] Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error seeding database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[Error] Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"[Error] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        private async Task EnsureDatabaseCreatedAsync()
        {
            try
            {
                // Periksa apakah database sudah ada, jika belum maka buat
                bool created = await _dbContext.Database.EnsureCreatedAsync();
                if (created)
                {
                    Console.WriteLine("[Info] Database was created successfully");
                }
                else
                {
                    Console.WriteLine("[Info] Database already exists");
                    
                    // Pastikan skema database terbaru dengan menjalankan migrasi yang belum diterapkan
                    try 
                    {
                        if (_dbContext.Database.GetPendingMigrations().Any())
                        {
                            Console.WriteLine("[Info] Applying pending migrations...");
                            await _dbContext.Database.MigrateAsync();
                            Console.WriteLine("[Info] Migrations applied successfully");
                        }
                        else
                        {
                            Console.WriteLine("[Info] Database schema is up to date");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Warning] Error checking/applying migrations: {ex.Message}. Will continue with existing schema.");
                    }
                }
                
                // Periksa apakah tabel utama sudah ada dan dapat diakses
                Console.WriteLine("[Info] Verifying database tables...");
                try
                {
                    // Coba mengakses tabel-tabel utama dan hitung jumlah entri
                    int settingsCount = await _dbContext.Settings.CountAsync();
                    int usersCount = await _dbContext.Users.CountAsync();
                    int rolesCount = await _dbContext.Roles.CountAsync();
                    int activitiesCount = await _dbContext.ParkingActivities.CountAsync();
                    int logsCount = await _dbContext.Logs.CountAsync();
                    
                    Console.WriteLine($"[Info] Database tables verified: Found {settingsCount} settings, {usersCount} users, {rolesCount} roles, {activitiesCount} activities, {logsCount} logs");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Warning] Could not verify tables: {ex.Message}");
                    
                    // Jika tidak bisa mengakses tabel, coba buat database dari awal
                    Console.WriteLine("[Info] Attempting to recreate database schema...");
                    
                    // Hapus database jika ada
                    await _dbContext.Database.EnsureDeletedAsync();
                    Console.WriteLine("[Info] Existing database deleted");
                    
                    // Buat database baru dengan schema terkini
                    await _dbContext.Database.EnsureCreatedAsync();
                    Console.WriteLine("[Info] New database created with current schema");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Database initialization error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        
        private async Task SeedRolesAsync()
        {
            if (!await _dbContext.Roles.AnyAsync())
            {
                var roles = new Role[]
                {
                    new Role { Name = "Admin", Description = "System Administrator", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Role { Name = "Manager", Description = "Parking Facility Manager", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Role { Name = "Operator", Description = "Parking Facility Operator", IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                
                await _dbContext.Roles.AddRangeAsync(roles);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        private async Task SeedAdminUserAsync()
        {
            if (!await _dbContext.Users.AnyAsync())
            {
                // Get admin role
                var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    throw new InvalidOperationException("Admin role not found. Ensure roles are seeded first.");
                }
                
                // Create admin password (using the same hash method as in UserService)
                string password = "admin123";
                string salt = Guid.NewGuid().ToString();
                string passwordHash = HashPassword(password + salt);
                
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@parkinglot.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    RoleId = adminRole.Id,
                    Role = adminRole,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = passwordHash,
                    PasswordSalt = salt
                };
                
                await _dbContext.Users.AddAsync(adminUser);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        private async Task SeedDefaultSettingsAsync()
        {
            if (!await _dbContext.Settings.AnyAsync())
            {
                var settings = new Setting[]
                {
                    new Setting { Key = "total_spots", Value = "100", Description = "Total parking spots available" },
                    new Setting { Key = "car_rate", Value = "5000", Description = "Parking rate for cars (hourly)" },
                    new Setting { Key = "motorcycle_rate", Value = "2000", Description = "Parking rate for motorcycles (hourly)" },
                    new Setting { Key = "truck_rate", Value = "10000", Description = "Parking rate for trucks (hourly)" },
                    new Setting { Key = "bus_rate", Value = "15000", Description = "Parking rate for buses (hourly)" },
                    new Setting { Key = "company_name", Value = "Parking Management System", Description = "Company name for reports" },
                    new Setting { Key = "company_address", Value = "123 Main Street", Description = "Company address for reports" },
                    new Setting { Key = "report_footer", Value = "Thank you for your business!", Description = "Footer text for reports" }
                };
                
                await _dbContext.Settings.AddRangeAsync(settings);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        private async Task SeedSampleParkingActivitiesAsync()
        {
            try
            {
                Console.WriteLine("[Info] Checking if parking activities data needs to be seeded...");
                
                // Only add sample data if no parking activities exist
                if (!await _dbContext.ParkingActivities.AnyAsync())
                {
                    Console.WriteLine("[Info] No existing parking activities found. Creating sample data...");
                    
                    var now = DateTime.Now;
                    var yesterday = now.AddDays(-1);
                    var twoDaysAgo = now.AddDays(-2);
                    var lastWeek = now.AddDays(-6);
                    
                    // Create sample entry and exit activities for different vehicle types
                    var activities = new List<ParkingActivity>();
                    
                    Console.WriteLine("[Info] Creating sample car entries and exits...");
                    
                    // Car entries and exits (some still parked, some already left)
                    for (int i = 1; i <= 10; i++)
                    {
                        // Generate plate number
                        string plateNumber = $"B {i.ToString().PadLeft(4, '0')} ABC";
                        
                        // Sample entry
                        var entryTime = now.AddHours(-new Random().Next(1, 8));
                        var entry = new ParkingActivity
                        {
                            VehicleNumber = plateNumber,
                            VehicleType = "Car",
                            Action = "Entry",
                            EntryTime = entryTime,
                            ExitTime = null,
                            Duration = null,
                            Fee = null,
                            Notes = "Regular customer",
                            Barcode = Guid.NewGuid().ToString().Substring(0, 8),
                            CreatedAt = entryTime,
                            FormattedTime = entryTime.ToString("HH:mm:ss")
                        };
                        activities.Add(entry);
                        
                        // For 60% of cars, also add exit records
                        if (i <= 6)
                        {
                            var exitTime = entryTime.AddHours(new Random().Next(1, 4));
                            var duration = (exitTime - entryTime).TotalHours;
                            var fee = Math.Round(duration * 5000, 0); // Car rate is 5000 per hour
                            
                            var exit = new ParkingActivity
                            {
                                VehicleNumber = plateNumber,
                                VehicleType = "Car",
                                Action = "Exit",
                                EntryTime = entryTime,
                                ExitTime = exitTime,
                                Duration = $"{duration:0.0} hours",
                                Fee = (decimal)fee,
                                Notes = "Regular customer",
                                Barcode = entry.Barcode,
                                CreatedAt = exitTime,
                                FormattedTime = exitTime.ToString("HH:mm:ss")
                            };
                            activities.Add(exit);
                        }
                    }
                    
                    Console.WriteLine("[Info] Creating sample motorcycle entries and exits...");
                    
                    // Motorcycle entries and exits
                    for (int i = 1; i <= 15; i++)
                    {
                        string plateNumber = $"B {i.ToString().PadLeft(4, '0')} XYZ";
                        
                        var entryTime = now.AddHours(-new Random().Next(1, 10));
                        var entry = new ParkingActivity
                        {
                            VehicleNumber = plateNumber,
                            VehicleType = "Motorcycle",
                            Action = "Entry",
                            EntryTime = entryTime,
                            ExitTime = null,
                            Duration = null,
                            Fee = null,
                            Notes = "Regular customer",
                            Barcode = Guid.NewGuid().ToString().Substring(0, 8),
                            CreatedAt = entryTime,
                            FormattedTime = entryTime.ToString("HH:mm:ss")
                        };
                        activities.Add(entry);
                        
                        // For 70% of motorcycles, also add exit records
                        if (i <= 10)
                        {
                            var exitTime = entryTime.AddHours(new Random().Next(1, 6));
                            var duration = (exitTime - entryTime).TotalHours;
                            var fee = Math.Round(duration * 2000, 0); // Motorcycle rate is 2000 per hour
                            
                            var exit = new ParkingActivity
                            {
                                VehicleNumber = plateNumber,
                                VehicleType = "Motorcycle",
                                Action = "Exit",
                                EntryTime = entryTime,
                                ExitTime = exitTime,
                                Duration = $"{duration:0.0} hours",
                                Fee = (decimal)fee,
                                Notes = "Regular customer",
                                Barcode = entry.Barcode,
                                CreatedAt = exitTime,
                                FormattedTime = exitTime.ToString("HH:mm:ss")
                            };
                            activities.Add(exit);
                        }
                    }
                    
                    Console.WriteLine("[Info] Creating sample truck and bus entries...");
                    
                    // Add a few trucks and buses for variety
                    string[] truckPlates = { "B 9876 XYZ", "B 8765 XYZ", "B 7654 XYZ" };
                    foreach (var plate in truckPlates)
                    {
                        var entryTime = now.AddHours(-new Random().Next(1, 5));
                        var entry = new ParkingActivity
                        {
                            VehicleNumber = plate,
                            VehicleType = "Truck",
                            Action = "Entry",
                            EntryTime = entryTime,
                            ExitTime = null,
                            Duration = null,
                            Fee = null,
                            Notes = "Commercial vehicle",
                            Barcode = Guid.NewGuid().ToString().Substring(0, 8),
                            CreatedAt = entryTime,
                            FormattedTime = entryTime.ToString("HH:mm:ss")
                        };
                        activities.Add(entry);
                        
                        // Add exit for one of the trucks
                        if (plate == "B 9876 XYZ")
                        {
                            var exitTime = entryTime.AddHours(2.5);
                            var duration = (exitTime - entryTime).TotalHours;
                            var fee = Math.Round(duration * 10000, 0); // Truck rate is 10000 per hour
                            
                            var exit = new ParkingActivity
                            {
                                VehicleNumber = plate,
                                VehicleType = "Truck",
                                Action = "Exit",
                                EntryTime = entryTime,
                                ExitTime = exitTime,
                                Duration = $"{duration:0.0} hours",
                                Fee = (decimal)fee,
                                Notes = "Commercial vehicle",
                                Barcode = entry.Barcode,
                                CreatedAt = exitTime,
                                FormattedTime = exitTime.ToString("HH:mm:ss")
                            };
                            activities.Add(exit);
                        }
                    }
                    
                    // Add a bus
                    var busEntryTime = now.AddHours(-3);
                    var busEntry = new ParkingActivity
                    {
                        VehicleNumber = "B 1234 BUS",
                        VehicleType = "Bus",
                        Action = "Entry",
                        EntryTime = busEntryTime,
                        ExitTime = null,
                        Duration = null,
                        Fee = null,
                        Notes = "Tour bus",
                        Barcode = Guid.NewGuid().ToString().Substring(0, 8),
                        CreatedAt = busEntryTime,
                        FormattedTime = busEntryTime.ToString("HH:mm:ss")
                    };
                    activities.Add(busEntry);
                    
                    Console.WriteLine($"[Info] Saving {activities.Count} parking activities to database...");
                    
                    // Add data to database - simpan satu per satu untuk menghindari error batch
                    int counter = 0;
                    foreach (var activity in activities)
                    {
                        try {
                            _dbContext.ParkingActivities.Add(activity);
                            await _dbContext.SaveChangesAsync();
                            counter++;
                            if (counter % 5 == 0)
                            {
                                Console.WriteLine($"[Info] Saved {counter} of {activities.Count} parking activities");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Error] Failed to save activity {activity.VehicleNumber}: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"[Info] Successfully added {counter} sample parking activities");
                }
                else
                {
                    Console.WriteLine("[Info] Parking activities already exist, skipping seed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error seeding parking activities: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        
        private async Task SeedSampleLogsAsync()
        {
            try
            {
                Console.WriteLine("[Info] Checking if logs data needs to be seeded...");
                
                // Only add sample logs if no logs exist
                if (!await _dbContext.Logs.AnyAsync())
                {
                    Console.WriteLine("[Info] No existing logs found. Creating sample logs...");
                    
                    var now = DateTime.Now;
                    var logs = new List<Log>
                    {
                        new Log { Level = LogLevel.Info, Message = "System initialized", Timestamp = now.AddMinutes(-60) },
                        new Log { Level = LogLevel.Info, Message = "Database connection established", Timestamp = now.AddMinutes(-59) },
                        new Log { Level = LogLevel.Info, Message = "User admin logged in", Username = "admin", Timestamp = now.AddMinutes(-55) },
                        new Log { Level = LogLevel.Info, Message = "Dashboard initialized", Timestamp = now.AddMinutes(-50) },
                        new Log { Level = LogLevel.Info, Message = "Vehicle B 1234 ABC entered", Timestamp = now.AddMinutes(-45) },
                        new Log { Level = LogLevel.Info, Message = "Vehicle B 5678 XYZ entered", Timestamp = now.AddMinutes(-40) },
                        new Log { Level = LogLevel.Warning, Message = "Low disk space detected", Timestamp = now.AddMinutes(-35) },
                        new Log { Level = LogLevel.Info, Message = "Vehicle B 1234 ABC exited", Timestamp = now.AddMinutes(-30) },
                        new Log { Level = LogLevel.Error, Message = "Failed to process payment", Timestamp = now.AddMinutes(-25) },
                        new Log { Level = LogLevel.Info, Message = "System backup completed", Timestamp = now.AddMinutes(-20) },
                        new Log { Level = LogLevel.Info, Message = "Vehicle B 9876 BUS entered", Timestamp = now.AddMinutes(-15) },
                        new Log { Level = LogLevel.Info, Message = "Settings updated", Username = "admin", Timestamp = now.AddMinutes(-10) },
                        new Log { Level = LogLevel.Info, Message = "Report generated", Username = "admin", Timestamp = now.AddMinutes(-5) }
                    };
                    
                    Console.WriteLine($"[Info] Saving {logs.Count} logs to database...");
                    
                    // Add logs satu per satu
                    int counter = 0;
                    foreach (var log in logs)
                    {
                        try
                        {
                            _dbContext.Logs.Add(log);
                            await _dbContext.SaveChangesAsync();
                            counter++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Error] Failed to save log: {ex.Message}");
                        }
                    }
                    
                    Console.WriteLine($"[Info] Successfully added {counter} sample logs");
                }
                else
                {
                    Console.WriteLine("[Info] Logs already exist, skipping seed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error seeding logs: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        
        private string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
} 