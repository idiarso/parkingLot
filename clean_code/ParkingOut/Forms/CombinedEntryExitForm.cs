using System;
using SimpleParkingAdmin.Utils;
using Serilog;

namespace SimpleParkingAdmin.Forms
{
    public class CombinedEntryExitForm
    {
        private readonly IAppLogger _logger;

        public CombinedEntryExitForm()
        {
            _logger = new FileLogger();
        }

        public void RecordVehicleEntry()
        {
            _logger.Information("Vehicle entry recorded successfully");
        }

        public void RecordVehicleEntryFailure(Exception ex)
        {
            _logger.Error("Failed to record vehicle entry", ex);
        }

        public void VehicleNotFound()
        {
            _logger.Warning("Vehicle not found in database");
        }

        public void ProcessVehicleExit()
        {
            _logger.Debug("Processing vehicle exit");
        }
    }
} 