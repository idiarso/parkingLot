namespace SimpleParkingAdmin.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public int VehicleTypeId { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string EntryImagePath { get; set; } = string.Empty;
    public string? ExitImagePath { get; set; }
    public decimal? Fee { get; set; }
    public int? PaymentMethodId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
} 