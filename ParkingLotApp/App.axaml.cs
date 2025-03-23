using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using ParkingLotApp.ViewModels;
using ParkingLotApp.Views;
using ParkingLotApp.Data;
using ParkingLotApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using ParkingLotApp.Services.Interfaces;
using System.Threading.Tasks;

namespace ParkingLotApp;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database and seed data
            Task.Run(async () => await InitializeDatabaseAsync()).Wait();

            // Avoid duplicate validations
            DisableAvaloniaDataAnnotationValidation();

            // Create main window with view model from DI
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            // Get database context from DI
            using var scope = _serviceProvider?.CreateScope();
            var dbContext = scope?.ServiceProvider.GetRequiredService<ParkingDbContext>();
            
            if (dbContext != null)
            {
                // Explicitly create the database and schema instead of using migrations
                // which appear to be causing issues
                await dbContext.Database.EnsureCreatedAsync();
                
                Console.WriteLine("Database created successfully");
                
                // Seed database with initial data
                var seeder = new DatabaseSeeder(dbContext);
                await seeder.SeedAsync();
                
                Console.WriteLine("Database initialization completed successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Get connection string from App.config
        var connectionString = ConfigurationManager.ConnectionStrings["ParkingDBConnection"]?.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ParkingDBConnection' not found in App.config");
        }

        // Register database context with options
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        // Register services
        services.AddSingleton<IParkingService, ParkingService>();
        services.AddSingleton<ILogger, Logger>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ISettingsService>(provider => 
            new SettingsService(provider));
        services.AddSingleton<IReportService, ReportService>();
        
        // DashboardService needs access to IServiceProvider for thread-safe DbContext access
        services.AddSingleton<DashboardService>(provider => 
            new DashboardService(
                provider, 
                provider.GetRequiredService<ISettingsService>(),
                provider.GetRequiredService<ILogger>()
            ));
        
        // Register view models with updated constructor parameters
        services.AddTransient<MainWindowViewModel>(provider =>
            new MainWindowViewModel(
                provider.GetRequiredService<ParkingDbContext>(),
                provider.GetRequiredService<IParkingService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<ISettingsService>(),
                provider.GetRequiredService<IReportService>(),
                provider.GetRequiredService<DashboardService>(),
                provider.GetRequiredService<ILogger>(),
                provider
            ));
        
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>(provider =>
            new DashboardViewModel(
                provider.GetRequiredService<IParkingService>(),
                provider.GetRequiredService<ISettingsService>(),
                provider.GetRequiredService<DashboardService>(),
                provider.GetRequiredService<ILogger>(),
                provider
            ));
        services.AddTransient<VehicleEntryViewModel>();
        services.AddTransient<VehicleExitViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<LogViewerViewModel>(provider =>
            new LogViewerViewModel(
                provider,
                provider.GetRequiredService<ILogger>()
            ));
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}