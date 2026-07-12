namespace DeliveryManagementApp.Application.Couriers.DTOs;

public class CourierDto
{
    public int CourierId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
}
