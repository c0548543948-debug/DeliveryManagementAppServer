using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Orders.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string OriginAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public DateOnly RequiredDate { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public string? DeliveryImageUrl { get; set; }
    public TimeOnly? TimeWindowStart { get; set; }
    public TimeOnly? TimeWindowEnd { get; set; }
}
