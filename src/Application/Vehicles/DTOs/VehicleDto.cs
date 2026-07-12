using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Vehicles.DTOs;

public class VehicleDto
{
    public int VehicleId { get; set; }
    public VehicleType Type { get; set; }
    public decimal CapacityVolume { get; set; }
    public decimal CapacityWeight { get; set; }
    public string LicensePlate { get; set; } = string.Empty;

}
