using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public int CustomerId { get; set; }
    public string OriginAddress { get; set; } = null!;
    public string DestinationAddress { get; set; } = null!;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public DateOnly RequiredDate { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
    public string? DeliveryImageURL { get; set; }

    // Time window for delivery (customer availability)
    public TimeOnly? TimeWindowStart { get; set; }
    public TimeOnly? TimeWindowEnd { get; set; }

    // Minutes needed at the stop (unloading, signature, etc.)
    public int ServiceTimeMinutes { get; set; } = 10;

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<RouteItem> RouteItems { get; set; } = new List<RouteItem>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
