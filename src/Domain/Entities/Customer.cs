namespace DeliveryManagementApp.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string ApplicationUserId { get; set; } = null!;
    public string Phone { get; set; } = null!;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
