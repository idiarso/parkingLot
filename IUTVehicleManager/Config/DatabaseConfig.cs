using System;
using System.Configuration;

namespace IUTVehicleManager.Config
{
    public static class DatabaseConfig
    {
        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString
                ?? throw new ConfigurationErrorsException($"Connection string '{name}' not found.");
        }

        public static string GetTerminalType()
        {
            return ConfigurationManager.AppSettings["TerminalType"]
                ?? throw new ConfigurationErrorsException("TerminalType not configured.");
        }

        public static int GetSyncInterval()
        {
            string interval = ConfigurationManager.AppSettings["SyncInterval"] ?? "30000";
            return int.Parse(interval);
        }

        public static int GetMaxRetries()
        {
            string retries = ConfigurationManager.AppSettings["MaxRetries"] ?? "3";
            return int.Parse(retries);
        }

        public static string GetPrinterPort()
        {
            return ConfigurationManager.AppSettings["PrinterPort"]
                ?? throw new ConfigurationErrorsException("PrinterPort not configured.");
        }

        public static string GetGatePort()
        {
            return ConfigurationManager.AppSettings["GatePort"]
                ?? throw new ConfigurationErrorsException("GatePort not configured.");
        }
    }
} 