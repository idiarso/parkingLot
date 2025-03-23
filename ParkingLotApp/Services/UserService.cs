using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using ParkingLotApp.Models;
using ParkingLotApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services
{
    public class UserService : IUserService
    {
        private readonly ParkingDbContext _dbContext;
        private readonly ILogger _logger;
        private User? _currentUser;

        public UserService(ParkingDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.Role)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to get all users", ex);
                return new List<User>();
            }
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (user != null)
                {
                    // Verificar password utilizando la sal almacenada
                    var passwordHash = HashPassword(password + user.PasswordSalt);
                    if (passwordHash == user.PasswordHash)
                    {
                        user.LastLoginAt = DateTime.Now;
                        await _dbContext.SaveChangesAsync();
                        
                        // Store the current user
                        _currentUser = user;
                        
                        await _logger.LogInfoAsync($"Login successful: {username} (User ID: {user.Id})");
                        return user;
                    }
                    else
                    {
                        await _logger.LogInfoAsync($"Failed login attempt for user '{username}'.");
                    }
                }

                await _logger.LogWarningAsync($"Failed login attempt: {username} (Invalid credentials)");
                return new User();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Authentication failed for user {username}", ex);
                return new User();
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);
                
                return user ?? new User();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to get user with ID {id}", ex);
                return new User();
            }
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();

                // Set password hash
                user.PasswordHash = HashPassword(password);
                
                // Tambahkan user ke database
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                
                // Log aktivitas
                await _logger.LogInfoAsync($"User created: {user.Username}");
                
                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to create user: {user.Username}", ex);
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(User user, string? password = null)
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();
                
                // Update password jika ada
                if (!string.IsNullOrEmpty(password))
                {
                    user.PasswordHash = HashPassword(password);
                }
                
                // Update user di database
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                
                // Log aktivitas
                await _logger.LogInfoAsync($"User updated: {user.Username}");
                
                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to update user: {user.Username}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();
                
                // Cari user
                var user = await _dbContext.Users.FindAsync(id);
                if (user == null)
                    return false;

                // Hapus user dari database
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                
                // Log aktivitas
                await _logger.LogInfoAsync($"User deleted: ID {id}");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to delete user with ID {id}", ex);
                return false;
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();
                
                // Ambil semua role
                return await _dbContext.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to get all roles", ex);
                return new List<Role>();
            }
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();
                
                // Cari role
                var role = await _dbContext.Roles.FindAsync(id);
                return role ?? new Role();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to get role with ID {id}", ex);
                return new Role();
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                // Pastikan database terkoneksi
                await _dbContext.EnsureConnectedAsync();
                
                // Cari user
                var user = await _dbContext.Users.FindAsync(userId);
                
                // Validasi password saat ini
                if (user == null || user.PasswordHash != HashPassword(currentPassword))
                    return false;

                // Update password
                user.PasswordHash = HashPassword(newPassword);
                await _dbContext.SaveChangesAsync();
                
                // Log aktivitas
                await _logger.LogInfoAsync($"Password changed for user ID {userId}");
                
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to change password for user ID {userId}", ex);
                return false;
            }
        }

        public async Task<List<Shift>> GetAllShiftsAsync()
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();
                var shifts = new List<Shift>();

                using var cmd = new NpgsqlCommand(
                    @"SELECT id, name, start_time, end_time, description, created_at, updated_at 
                      FROM shifts 
                      ORDER BY start_time",
                    _dbContext.Connection);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    shifts.Add(new Shift
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        StartTime = reader.GetTimeSpan(2),
                        EndTime = reader.GetTimeSpan(3),
                        Description = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                        CreatedAt = reader.GetDateTime(5),
                        UpdatedAt = !reader.IsDBNull(6) ? reader.GetDateTime(6) : null
                    });
                }

                return shifts;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to get shifts", ex);
                return new List<Shift>();
            }
        }

        public async Task<bool> CreateShiftAsync(Shift shift)
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();
                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO shifts (name, start_time, end_time, description, created_at) 
                      VALUES (@name, @startTime, @endTime, @description, @createdAt)",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("name", shift.Name);
                cmd.Parameters.AddWithValue("startTime", shift.StartTime);
                cmd.Parameters.AddWithValue("endTime", shift.EndTime);
                cmd.Parameters.AddWithValue("description", shift.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("createdAt", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to create shift: {shift.Name}", ex);
                return false;
            }
        }

        public async Task<bool> UpdateShiftAsync(Shift shift)
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();
                using var cmd = new NpgsqlCommand(
                    @"UPDATE shifts 
                      SET name = @name,
                          start_time = @startTime,
                          end_time = @endTime,
                          description = @description,
                          updated_at = @updatedAt
                      WHERE id = @shiftId",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("shiftId", shift.Id);
                cmd.Parameters.AddWithValue("name", shift.Name);
                cmd.Parameters.AddWithValue("startTime", shift.StartTime);
                cmd.Parameters.AddWithValue("endTime", shift.EndTime);
                cmd.Parameters.AddWithValue("description", shift.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("updatedAt", DateTime.Now);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to update shift: {shift.Name}", ex);
                return false;
            }
        }

        public async Task<bool> DeleteShiftAsync(int shiftId)
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();
                using var cmd = new NpgsqlCommand(
                    "DELETE FROM shifts WHERE id = @shiftId",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("shiftId", shiftId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to delete shift with ID {shiftId}", ex);
                return false;
            }
        }

        public async Task<List<UserShift>> GetUserShiftsAsync(DateTime date)
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();
                var userShifts = new List<UserShift>();

                using var cmd = new NpgsqlCommand(
                    @"SELECT us.id, us.user_id, us.shift_id, us.assigned_date, us.created_at,
                             u.username, u.first_name, u.last_name, r.name as role_name,
                             s.name, s.start_time, s.end_time, s.description
                      FROM user_shifts us
                      JOIN users u ON us.user_id = u.id
                      JOIN roles r ON u.role_id = r.id
                      JOIN shifts s ON us.shift_id = s.id
                      WHERE DATE(us.assigned_date) = @date
                      ORDER BY s.start_time",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("date", date.Date);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new User
                    {
                        Id = reader.GetInt32(1),
                        Username = reader.GetString(5),
                        FirstName = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                        LastName = !reader.IsDBNull(7) ? reader.GetString(7) : string.Empty,
                        Role = new Role { Name = !reader.IsDBNull(8) ? reader.GetString(8) : "User" }
                    };

                    var shift = new Shift
                    {
                        Id = reader.GetInt32(2),
                        Name = reader.GetString(9),
                        StartTime = reader.GetTimeSpan(10),
                        EndTime = reader.GetTimeSpan(11),
                        Description = !reader.IsDBNull(12) ? reader.GetString(12) : string.Empty
                    };

                    var userShift = new UserShift
                    {
                        Id = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        ShiftId = reader.GetInt32(2),
                        AssignedDate = reader.GetDateTime(3),
                        CreatedAt = reader.GetDateTime(4),
                        User = user,
                        Shift = shift
                    };

                    userShifts.Add(userShift);
                }

                return userShifts;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to get user shifts for date {date.ToShortDateString()}", ex);
                return new List<UserShift>();
            }
        }

        public async Task<bool> AssignShiftAsync(int userId, int shiftId, DateTime date)
        {
            try
            {
                _dbContext.EnsureConnected();
                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO user_shifts (user_id, shift_id, assigned_date, created_at)
                      VALUES (@userId, @shiftId, @assignedDate, @createdAt)
                      ON CONFLICT (user_id, shift_id, assigned_date) DO NOTHING",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("shiftId", shiftId);
                cmd.Parameters.AddWithValue("assignedDate", date.Date);
                cmd.Parameters.AddWithValue("createdAt", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveShiftAssignmentAsync(int userId, int shiftId, DateTime date)
        {
            try
            {
                _dbContext.EnsureConnected();
                using var cmd = new NpgsqlCommand(
                    @"DELETE FROM user_shifts 
                      WHERE user_id = @userId 
                      AND shift_id = @shiftId 
                      AND DATE(assigned_date) = @date",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("shiftId", shiftId);
                cmd.Parameters.AddWithValue("date", date.Date);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser == null)
            {
                return new User();
            }
            
            // Refresh user data from database
            var freshUser = await GetUserByIdAsync(_currentUser.Id);
            if (freshUser != null && freshUser.Id > 0)
            {
                _currentUser = freshUser;
                return freshUser;
            }
            
            return new User();
        }

        public async Task LogoutAsync()
        {
            if (_currentUser != null)
            {
                // Log the logout
                await _logger.LogInfoAsync($"User {_currentUser.Username} logged out");
                
                // Clear the current user
                _currentUser = null;
            }
            
            await Task.CompletedTask;
        }
    }
} 