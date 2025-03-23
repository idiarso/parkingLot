using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TestWpfApp.Models;
using TestWpfApp.Services;
using TestWpfApp.Services.Interfaces;
using System.Threading.Tasks;

namespace TestWpfApp.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly IAppLogger _logger;
        private readonly IDatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            
            // Initialize logger
            _logger = App.Logger;
            
            // Initialize database service
            _databaseService = new DatabaseService(_logger);
            
            // Ensure user table exists and create admin user if needed
            InitializeDatabaseAsync();
            
            // Set focus to username field when page loads
            Loaded += (s, e) => txtUsername.Focus();
            
            // Allow pressing Enter in password field to login
            txtPassword.KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                    AttemptLogin();
            };
        }

        private async void InitializeDatabaseAsync()
        {
            try
            {
                // Ensure user table exists and create admin user if needed
                await _databaseService.EnsureUserTableExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing database: {ex.Message}", ex);
                MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private async void AttemptLogin()
        {
            // Disable login button and show loading indicator
            btnLogin.IsEnabled = false;
            loginProgress.Visibility = Visibility.Visible;
            errorMessage.Visibility = Visibility.Collapsed;
            
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("Please enter both username and password.");
                    return;
                }

                // Authenticate user with database
                var user = await _databaseService.AuthenticateUserAsync(username, password);
                
                if (user != null)
                {
                    // Authentication successful - update last login time
                    await _databaseService.UpdateLastLoginAsync(user);
                    
                    // Store the logged-in user information
                    UserSession.CurrentUser = user;
                    UserSession.IsUserLoggedIn = true;
                    
                    _logger.Info($"User {user.Username} logged in successfully");
                    
                    // Navigate to Dashboard Page
                    NavigationService?.Navigate(new DashboardPage(_logger, _databaseService));
                }
                else
                {
                    // Authentication failed
                    ShowError("Invalid username or password. Please try again.");
                    _logger.Warning($"Failed login attempt for user: {username}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error: {ex.Message}", ex);
                ShowError($"An error occurred during login: {ex.Message}");
            }
            finally
            {
                // Re-enable login button and hide loading indicator
                btnLogin.IsEnabled = true;
                loginProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            errorMessage.Text = message;
            errorMessage.Visibility = Visibility.Visible;
        }

        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact your system administrator to reset your password.", "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
