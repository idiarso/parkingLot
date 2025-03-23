using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Models;
using System;
using System.Threading.Tasks;
using Npgsql;

namespace ParkingLotApp.Data
{
    public class ParkingDbContext : DbContext
    {
        private NpgsqlConnection? _connection;
        public NpgsqlConnection Connection => _connection ?? throw new InvalidOperationException("Connection not established");

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<ParkingActivity> ParkingActivities { get; set; } = null!;
        public DbSet<Setting> Settings { get; set; } = null!;
        public DbSet<Shift> Shifts { get; set; } = null!;
        public DbSet<UserShift> UserShifts { get; set; } = null!;
        public DbSet<Log> Logs { get; set; } = null!;

        public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
            : base(options)
        {
            // Constructor without Database.EnsureCreated() to avoid conflicts with migrations
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email);

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ParkingActivity configuration
            modelBuilder.Entity<ParkingActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehicleNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.VehicleType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(10);
                entity.Property(e => e.EntryTime).IsRequired();
                entity.Property(e => e.Fee).HasPrecision(10, 2);
                
                entity.HasIndex(e => e.VehicleNumber);
                entity.HasIndex(e => e.EntryTime);
                entity.HasIndex(e => e.ExitTime);
                entity.HasIndex(e => e.Barcode).IsUnique();
            });

            // Setting configuration
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Value).IsRequired();
            });

            // Shift configuration
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // UserShift configuration
            modelBuilder.Entity<UserShift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ShiftId, e.AssignedDate }).IsUnique();
                
                entity.HasOne(us => us.User)
                    .WithMany()
                    .HasForeignKey(us => us.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(us => us.Shift)
                    .WithMany()
                    .HasForeignKey(us => us.ShiftId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public async Task EnsureConnectedAsync()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new NpgsqlConnection(Database.GetConnectionString());
                await _connection.OpenAsync();
            }
        }

        public void EnsureConnected()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new NpgsqlConnection(Database.GetConnectionString());
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public override void Dispose()
        {
            CloseConnection();
            base.Dispose();
        }
    }
} 