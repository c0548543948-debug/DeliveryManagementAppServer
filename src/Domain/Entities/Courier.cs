namespace DeliveryManagementApp.Domain.Entities;

public class Courier : BaseAuditableEntity
{
    public string ApplicationUserId { get; set; } = null!;

    // Navigation
    public ICollection<Route> Routes { get; set; } = new List<Route>();
}
