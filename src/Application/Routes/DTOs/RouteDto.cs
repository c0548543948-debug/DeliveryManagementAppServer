using DeliveryManagementApp.Application.RouteItems.DTOs;

namespace DeliveryManagementApp.Application.Routes.DTOs;

public class RouteDto
{
    public int RouteId { get; set; }
    public int CourierId { get; set; }
    public int? VehicleId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<RouteItemDto> Items { get; set; } = new();

    // Display fields — populated by the query handler
    public string CourierName { get; set; } = string.Empty;
    public string VehicleLicensePlate { get; set; } = string.Empty;
}
