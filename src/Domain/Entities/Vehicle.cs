using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Domain.Entities;

public class Vehicle : BaseAuditableEntity
{
    public VehicleType Type { get; set; }
    public decimal CapacityVolume { get; set; }
    public decimal CapacityWeight { get; set; }
    public string LicensePlate { get; set; } = string.Empty;

    // Navigation
    public ICollection<Route> Routes { get; set; } = new List<Route>();
}
