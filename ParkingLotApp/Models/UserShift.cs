using System;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class UserShift : ReactiveObject
    {
        private int _id;
        private int _userId;
        private int _shiftId;
        private DateTime _assignedDate;
        private DateTime _createdAt;
        private User? _user;
        private Shift? _shift;

        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public int UserId
        {
            get => _userId;
            set => this.RaiseAndSetIfChanged(ref _userId, value);
        }

        public int ShiftId
        {
            get => _shiftId;
            set => this.RaiseAndSetIfChanged(ref _shiftId, value);
        }

        public DateTime AssignedDate
        {
            get => _assignedDate;
            set => this.RaiseAndSetIfChanged(ref _assignedDate, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        public User? User
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
        }

        public Shift? Shift
        {
            get => _shift;
            set => this.RaiseAndSetIfChanged(ref _shift, value);
        }
    }
} 