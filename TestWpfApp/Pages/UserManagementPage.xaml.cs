using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for UserManagementPage.xaml
    /// </summary>
    public partial class UserManagementPage : Page
    {
        private ObservableCollection<User> _users;
        private User _selectedUser;
        private bool _isEditing = false;
        private string _searchText = string.Empty;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                FilterUsers();
            }
        }

        public UserManagementPage()
        {
            InitializeComponent();
            LoadSampleData();
            this.DataContext = this;
        }

        private void LoadSampleData()
        {
            // In a real app, this would load data from database
            _users = new ObservableCollection<User>
            {
                new User
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = "admin@parkingsystem.com",
                    Role = "Administrator",
                    Status = "Active",
                    IsActive = true,
                    Permissions = new UserPermissions
                    {
                        VehicleEntryManagement = true,
                        VehicleExitManagement = true,
                        GenerateReports = true,
                        ModifySettings = true,
                        UserManagement = true,
                        ShiftsManagement = true
                    }
                },
                new User
                {
                    Username = "manager",
                    FullName = "Parking Manager",
                    Email = "manager@parkingsystem.com",
                    Role = "Manager",
                    Status = "Active",
                    IsActive = true,
                    Permissions = new UserPermissions
                    {
                        VehicleEntryManagement = true,
                        VehicleExitManagement = true,
                        GenerateReports = true,
                        ModifySettings = false,
                        UserManagement = false,
                        ShiftsManagement = true
                    }
                },
                new User
                {
                    Username = "operator1",
                    FullName = "Parking Operator 1",
                    Email = "operator1@parkingsystem.com",
                    Role = "Operator",
                    Status = "Active",
                    IsActive = true,
                    Permissions = new UserPermissions
                    {
                        VehicleEntryManagement = true,
                        VehicleExitManagement = true,
                        GenerateReports = false,
                        ModifySettings = false,
                        UserManagement = false,
                        ShiftsManagement = false
                    }
                },
                new User
                {
                    Username = "cashier1",
                    FullName = "Parking Cashier 1",
                    Email = "cashier1@parkingsystem.com",
                    Role = "Cashier",
                    Status = "Active",
                    IsActive = true,
                    Permissions = new UserPermissions
                    {
                        VehicleEntryManagement = false,
                        VehicleExitManagement = true,
                        GenerateReports = false,
                        ModifySettings = false,
                        UserManagement = false,
                        ShiftsManagement = false
                    }
                },
                new User
                {
                    Username = "cashier2",
                    FullName = "Parking Cashier 2",
                    Email = "cashier2@parkingsystem.com",
                    Role = "Cashier",
                    Status = "Inactive",
                    IsActive = false,
                    Permissions = new UserPermissions
                    {
                        VehicleEntryManagement = false,
                        VehicleExitManagement = true,
                        GenerateReports = false,
                        ModifySettings = false,
                        UserManagement = false,
                        ShiftsManagement = false
                    }
                }
            };

            dgUsers.ItemsSource = _users;
            txtPageInfo.Text = $"Page 1 of 1 ({_users.Count} users)";
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                dgUsers.ItemsSource = _users;
            }
            else
            {
                var filteredUsers = _users.Where(u =>
                    u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                dgUsers.ItemsSource = filteredUsers;
                txtPageInfo.Text = $"Filtered: {filteredUsers.Count} users";
            }
        }

        private void ClearForm()
        {
            txtUsername.Text = string.Empty;
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            cmbRole.SelectedIndex = 0;
            txtPassword.Password = string.Empty;
            txtConfirmPassword.Password = string.Empty;
            chkActive.IsChecked = true;

            // Reset permissions
            chkPermVehicleEntry.IsChecked = true;
            chkPermVehicleExit.IsChecked = true;
            chkPermReports.IsChecked = false;
            chkPermSettings.IsChecked = false;
            chkPermUserManagement.IsChecked = false;
            chkPermShifts.IsChecked = false;

            _isEditing = false;
            txtFormTitle.Text = "Add New User";
            txtUsername.IsEnabled = true;
        }

        private void FillFormWithUser(User user)
        {
            if (user == null) return;

            txtUsername.Text = user.Username;
            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;

            // Set role in combobox
            foreach (ComboBoxItem item in cmbRole.Items)
            {
                if (item.Content.ToString() == user.Role)
                {
                    cmbRole.SelectedItem = item;
                    break;
                }
            }

            chkActive.IsChecked = user.IsActive;

            // Set permissions
            if (user.Permissions != null)
            {
                chkPermVehicleEntry.IsChecked = user.Permissions.VehicleEntryManagement;
                chkPermVehicleExit.IsChecked = user.Permissions.VehicleExitManagement;
                chkPermReports.IsChecked = user.Permissions.GenerateReports;
                chkPermSettings.IsChecked = user.Permissions.ModifySettings;
                chkPermUserManagement.IsChecked = user.Permissions.UserManagement;
                chkPermShifts.IsChecked = user.Permissions.ShiftsManagement;
            }

            _isEditing = true;
            txtFormTitle.Text = "Edit User";
            txtUsername.IsEnabled = false; // Username shouldn't be changed once created
        }

        private User GetUserFromForm()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please fill in all required fields", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (!_isEditing) // Creating a new user
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Password is required for new users", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Passwords do not match", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                // Check if username already exists
                if (_users.Any(u => u.Username.Equals(txtUsername.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Username already exists", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            else if (!string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                // If changing password during edit
                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Passwords do not match", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }

            var user = new User
            {
                Username = txtUsername.Text,
                FullName = txtFullName.Text,
                Email = txtEmail.Text,
                Role = ((ComboBoxItem)cmbRole.SelectedItem).Content.ToString(),
                IsActive = chkActive.IsChecked ?? false,
                Status = (chkActive.IsChecked ?? false) ? "Active" : "Inactive",
                Permissions = new UserPermissions
                {
                    VehicleEntryManagement = chkPermVehicleEntry.IsChecked ?? false,
                    VehicleExitManagement = chkPermVehicleExit.IsChecked ?? false,
                    GenerateReports = chkPermReports.IsChecked ?? false,
                    ModifySettings = chkPermSettings.IsChecked ?? false,
                    UserManagement = chkPermUserManagement.IsChecked ?? false,
                    ShiftsManagement = chkPermShifts.IsChecked ?? false
                }
            };

            // In a real app, we would hash the password and store it
            // Here we're just simulating the process
            if (!_isEditing || !string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                // Store password hash (simulated here)
                user.PasswordHash = "HASHED_" + txtPassword.Password;
            }
            else if (_isEditing && _selectedUser != null)
            {
                // Keep the existing password hash if not changing password
                user.PasswordHash = _selectedUser.PasswordHash;
            }

            return user;
        }

        #region Event Handlers

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = dgUsers.SelectedItem as User;
            if (_selectedUser != null)
            {
                FillFormWithUser(_selectedUser);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var user = button.Tag as User;
                if (user != null)
                {
                    _selectedUser = user;
                    FillFormWithUser(user);
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var user = button.Tag as User;
                if (user != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete user {user.Username}?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _users.Remove(user);
                        FilterUsers();
                        ClearForm();
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var user = GetUserFromForm();
            if (user == null) return;

            if (_isEditing)
            {
                // Update existing user
                var existingUser = _users.FirstOrDefault(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    // Copy properties from the form to the existing user
                    int index = _users.IndexOf(existingUser);
                    _users[index] = user;
                }
            }
            else
            {
                // Add new user
                _users.Add(user);
            }

            FilterUsers();
            ClearForm();
            MessageBox.Show("User saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            // In a real app with pagination, go to previous page
            MessageBox.Show("This would navigate to the previous page of users", "Pagination",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            // In a real app with pagination, go to next page
            MessageBox.Show("This would navigate to the next page of users", "Pagination",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public UserPermissions Permissions { get; set; }
    }

    public class UserPermissions
    {
        public bool VehicleEntryManagement { get; set; }
        public bool VehicleExitManagement { get; set; }
        public bool GenerateReports { get; set; }
        public bool ModifySettings { get; set; }
        public bool UserManagement { get; set; }
        public bool ShiftsManagement { get; set; }
    }
} 