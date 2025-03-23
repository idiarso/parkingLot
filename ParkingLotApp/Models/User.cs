using System;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class User : ReactiveObject
    {
        private int _id;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private int _roleId;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _lastLoginAt;
        private Role _role = null!;
        private string _passwordHash = string.Empty;
        private string _passwordSalt = string.Empty;
        private DateTime? _updatedAt;

        public int Id 
        { 
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string Username 
        { 
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password 
        { 
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string Email 
        { 
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string FirstName 
        { 
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        public string LastName 
        { 
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        public int RoleId 
        { 
            get => _roleId;
            set => this.RaiseAndSetIfChanged(ref _roleId, value);
        }

        public Role Role 
        { 
            get => _role;
            set => this.RaiseAndSetIfChanged(ref _role, value);
        }

        public bool IsActive 
        { 
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        public DateTime CreatedAt 
        { 
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        public DateTime? LastLoginAt 
        { 
            get => _lastLoginAt;
            set => this.RaiseAndSetIfChanged(ref _lastLoginAt, value);
        }

        public string PasswordHash 
        { 
            get => _passwordHash;
            set => this.RaiseAndSetIfChanged(ref _passwordHash, value);
        }

        public string PasswordSalt
        {
            get => _passwordSalt;
            set => this.RaiseAndSetIfChanged(ref _passwordSalt, value);
        }

        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set => this.RaiseAndSetIfChanged(ref _updatedAt, value);
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Username);

        public string FullName => $"{FirstName} {LastName}";

        public string DisplayName => $"{FirstName} {LastName}";
    }
} 