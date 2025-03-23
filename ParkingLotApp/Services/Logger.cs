using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ParkingLotApp.Data;
using ParkingLotApp.Models;
using ParkingLotApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ParkingLotApp.Services
{
    public class Logger : ILogger
    {
        private readonly ParkingDbContext _dbContext;
        private List<string> _memoryLogs = new List<string>();
        private bool _isDbInitialized = false;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Logger(ParkingDbContext dbContext)
        {
            _dbContext = dbContext;
            
            // Check database connection asynchronously
            Task.Run(async () =>
            {
                try
                {
                    // Wait a short time to let the application initialize the database
                    await Task.Delay(5000);
                    
                    var canConnect = await _dbContext.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        // Check if Logs table exists to confirm initialization
                        try
                        {
                            await _dbContext.Logs.FirstOrDefaultAsync();
                            _isDbInitialized = true;
                            Console.WriteLine("[Info] Logger database connection established");
                        }
                        catch
                        {
                            Console.WriteLine("[Warning] Logs table not available yet");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Logger initialization error: {ex.Message}");
                    // DB not ready yet, we'll keep using memory logs
                }
            });
        }

        public async Task LogInfoAsync(string message)
        {
            await LogAsync(LogLevel.Info, message);
        }

        public async Task LogWarningAsync(string message)
        {
            await LogAsync(LogLevel.Warning, message);
        }

        public async Task LogErrorAsync(string message, Exception? ex = null)
        {
            string details = ex?.ToString() ?? string.Empty;
            await LogAsync(LogLevel.Error, message, details);
        }

        public async Task LogLoginAttemptAsync(string username, bool success)
        {
            string message = success 
                ? $"User '{username}' logged in successfully." 
                : $"Failed login attempt for user '{username}'.";
                
            await LogAsync(LogLevel.Info, message, "AUTH");
        }

        private async Task LogAsync(LogLevel level, string message, string? details = null)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            Console.WriteLine(logEntry);
            
            // Always log to memory for immediate display
            lock (_memoryLogs)
            {
                _memoryLogs.Add(logEntry);
                if (_memoryLogs.Count > 100)
                {
                    _memoryLogs.RemoveAt(0);
                }
            }
            
            // Only try logging to DB if database is initialized
            if (_isDbInitialized)
            {
                try
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        var log = new Log
                        {
                            Level = level,
                            Message = message,
                            Details = details,
                            Timestamp = DateTime.Now
                        };
                        
                        _dbContext.Logs.Add(log);
                        await _dbContext.SaveChangesAsync();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error logging to database: {ex.Message}");
                    Console.WriteLine($"Original log: {logEntry}");
                }
            }
        }

        public async Task<List<Log>> GetRecentLogsAsync(int count = 100)
        {
            if (!_isDbInitialized)
            {
                // Return memory logs converted to Log objects
                return _memoryLogs.Take(count)
                    .Select(entry => new Log 
                    { 
                        Message = entry,
                        Level = entry.Contains("[ERROR]") ? LogLevel.Error : 
                                entry.Contains("[WARNING]") ? LogLevel.Warning : LogLevel.Info,
                        Timestamp = DateTime.Now
                    })
                    .ToList();
            }
            
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    return await _dbContext.Logs
                        .OrderByDescending(l => l.Timestamp)
                        .Take(count)
                        .ToListAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving logs from database: {ex.Message}");
                // Fallback to memory logs
                return _memoryLogs.Take(count)
                    .Select(entry => new Log 
                    { 
                        Message = entry,
                        Level = entry.Contains("[ERROR]") ? LogLevel.Error : 
                                entry.Contains("[WARNING]") ? LogLevel.Warning : LogLevel.Info,
                        Timestamp = DateTime.Now
                    })
                    .ToList();
            }
        }
    }
} 