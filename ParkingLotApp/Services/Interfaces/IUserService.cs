using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user, string? password = null);
        Task<bool> DeleteUserAsync(int id);
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<List<Shift>> GetAllShiftsAsync();
        Task<bool> CreateShiftAsync(Shift shift);
        Task<bool> UpdateShiftAsync(Shift shift);
        Task<bool> DeleteShiftAsync(int shiftId);
        Task<List<UserShift>> GetUserShiftsAsync(DateTime date);
        Task<bool> AssignShiftAsync(int userId, int shiftId, DateTime date);
        Task<bool> RemoveShiftAssignmentAsync(int userId, int shiftId, DateTime date);
        Task<User> GetCurrentUserAsync();
        Task LogoutAsync();
    }
} 