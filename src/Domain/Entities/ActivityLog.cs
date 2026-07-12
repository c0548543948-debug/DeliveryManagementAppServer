using DeliveryManagementApp.Domain.ValueObjects;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Domain.Entities;

public class ActivityLog : BaseAuditableEntity
{
    public int OrderId { get; set; }
    public string? ChangedByUserId { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public Location? LocationData { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
}
