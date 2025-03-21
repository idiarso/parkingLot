namespace SimpleParkingAdmin.Models;

public class ParkingRate
{
    public int Id { get; set; }
    public int VehicleTypeId { get; set; }
    public string? VehicleType { get; set; }  // For JOIN results
    public decimal FirstHourRate { get; set; }
    public decimal NextHourRate { get; set; }
    public decimal MaxDailyRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Calculate parking fee for a given duration
    public decimal CalculateFee(DateTime entryTime, DateTime exitTime)
    {
        decimal fee = 0;
        
        // Calculate total hours and days
        var totalHours = (int)(exitTime - entryTime).TotalHours;
        var days = totalHours / 24;
        var remainingHours = totalHours % 24;

        // Calculate fee for complete days
        if (days > 0)
            fee = days * MaxDailyRate;

        // Add fee for remaining hours
        if (remainingHours > 0)
        {
            // First hour
            fee += FirstHourRate;

            // Subsequent hours
            if (remainingHours > 1)
                fee += NextHourRate * (remainingHours - 1);

            // Cap at max daily rate
            if (fee > MaxDailyRate)
                fee = MaxDailyRate;
        }

        return fee;
    }

    // Format rate as string for display
    public override string ToString()
    {
        return $"{VehicleType}: First Hour: {FirstHourRate:C2}, Next Hours: {NextHourRate:C2}, Daily Max: {MaxDailyRate:C2}";
    }
} 