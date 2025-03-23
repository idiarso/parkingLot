using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TestWpfApp.Models;
using TestWpfApp.Services;
using System.Threading.Tasks;

namespace TestWpfApp.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly DatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            
            // Inisialisasi service database
            _databaseService = new DatabaseService();
            
            // Memastikan tabel user ada dan user admin tersedia
            InitializeDatabaseAsync();
            
            // Set focus to the username field when the page loads
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
                // Pastikan tabel user ada dan buat user admin default jika belum ada
                await _databaseService.EnsureUserTableExistsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private async void AttemptLogin()
        {
            // Disable login button dan tampilkan loading indicator
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

                // Autentikasi user dengan database
                var user = await _databaseService.AuthenticateUserAsync(username, password);
                
                if (user != null)
                {
                    // Authentication successful - update last login time
                    await _databaseService.UpdateLastLoginAsync(user.Id);
                    
                    // Store the logged-in user information
                    UserSession.CurrentUser = user;
                    UserSession.IsUserLoggedIn = true;

                    // Navigate to Dashboard Page
                    NavigationService?.Navigate(new DashboardPage());
                }
                else
                {
                    // Authentication failed
                    ShowError("Invalid username or password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login error: {ex.Message}");
            }
            finally
            {
                // Re-enable login button dan sembunyikan loading indicator
                btnLogin.IsEnabled = true;
                loginProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            errorMessage.Text = message;
            errorMessage.Visibility = Visibility.Visible;
            loginProgress.Visibility = Visibility.Collapsed;
            btnLogin.IsEnabled = true;
        }

        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please contact your system administrator to reset your password.", "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
