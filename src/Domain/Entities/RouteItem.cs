namespace DeliveryManagementApp.Domain.Entities;

public class RouteItem : BaseAuditableEntity
{
    public int RouteId { get; set; }
    public int OrderId { get; set; }
    public int StopOrder { get; set; }
    public StopType StopType { get; set; }
    public TimeOnly? EstimatedArrival { get; set; }

    // Navigation
    public Route Route { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
