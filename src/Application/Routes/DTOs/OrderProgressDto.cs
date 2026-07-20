namespace DeliveryManagementApp.Application.Routes.DTOs;

public class OrderProgressDto
{
    public int? PickupStop { get; set; }
    public int? DeliveryStop { get; set; }
    public int? CurrentStop { get; set; }
    public int? TotalStops { get; set; }
    public int? RouteId { get; set; }
}
