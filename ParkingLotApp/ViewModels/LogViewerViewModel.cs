using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using ParkingLotApp.Services;
using ParkingLotApp.Helpers;
using ReactiveUI;
using ParkingLotApp.Services.Interfaces;
using ParkingLotApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ParkingLotApp.ViewModels
{
    public class LogViewerViewModel : ViewModelBase, IDisposable
    {
        private ObservableCollection<string> _logEntries = new();
        private Timer _refreshTimer;
        private const int RefreshInterval = 1000; // 1 second
        private const int MaxLogEntries = 100;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ObservableCollection<string> LogEntries
        {
            get => _logEntries;
            set => this.RaiseAndSetIfChanged(ref _logEntries, value);
        }

        public LogViewerViewModel(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            _refreshTimer = new Timer(RefreshInterval);
            _refreshTimer.Elapsed += async (s, e) => await RefreshLogs();
            _refreshTimer.Start();

            // Initial load
            Task.Run(RefreshLogs);
        }

        private ParkingDbContext GetDbContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ParkingDbContext>();
        }

        private async Task RefreshLogs()
        {
            try
            {
                // Ambil log terbaru dari logger service
                var recentLogs = await _logger.GetRecentLogsAsync(MaxLogEntries);
                
                // Update pada UI thread
                await MainThreadHelper.InvokeOnMainThreadAsync(() =>
                {
                    LogEntries.Clear();
                    foreach (var log in recentLogs)
                    {
                        string logLevel = log.Level.ToString().ToUpper();
                        string timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                        string user = !string.IsNullOrEmpty(log.Username) ? $"[{log.Username}]" : "";
                        
                        LogEntries.Add($"[{logLevel}] {timestamp} {user} {log.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the error but don't display it to avoid recursive logging
                Console.WriteLine($"Error refreshing logs: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
    }
} 