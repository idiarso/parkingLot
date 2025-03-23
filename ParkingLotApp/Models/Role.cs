using System;
using System.Collections.Generic;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class Role : ReactiveObject
    {
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isActive = true;
        private DateTime _createdAt = DateTime.Now;
        private DateTime? _updatedAt;
        private ICollection<User> _users = new HashSet<User>();

        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
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

        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set => this.RaiseAndSetIfChanged(ref _updatedAt, value);
        }

        public virtual ICollection<User> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        public Role()
        {
            Users = new HashSet<User>();
        }

        public Role(string name, string description = "") : this()
        {
            Name = name;
            Description = description;
        }
    }
} 