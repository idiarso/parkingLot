using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Configuration;
using System;

namespace ParkingLotApp.Data
{
    public class ParkingDbContextFactory : IDesignTimeDbContextFactory<ParkingDbContext>
    {
        public ParkingDbContext CreateDbContext(string[] args)
        {
            // Load connection string from App.config
            var connectionString = ConfigurationManager.ConnectionStrings["ParkingDBConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback connection string for migrations
                connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=root@rsi;Password=1q2w3e4r5t;";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ParkingDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ParkingDbContext(optionsBuilder.Options);
        }
    }
} 