namespace SimpleParkingAdmin.Models;

public class Setting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Common setting keys
    public static class Keys
    {
        public const string EntryCamera = "EntryCamera";
        public const string ExitCamera = "ExitCamera";
        public const string ImagePath = "ImagePath";
        public const string SaveImages = "SaveImages";
        public const string EntryPrinter = "EntryPrinter";
        public const string ExitPrinter = "ExitPrinter";
        public const string EntryTicketHeader = "EntryTicketHeader";
        public const string ExitReceiptHeader = "ExitReceiptHeader";
        public const string FooterText = "FooterText";
    }

    // Helper methods for type conversion
    public bool GetBoolValue()
    {
        return bool.TryParse(Value, out bool result) && result;
    }

    public int GetIntValue(int defaultValue = 0)
    {
        return int.TryParse(Value, out int result) ? result : defaultValue;
    }

    public decimal GetDecimalValue(decimal defaultValue = 0)
    {
        return decimal.TryParse(Value, out decimal result) ? result : defaultValue;
    }
} 