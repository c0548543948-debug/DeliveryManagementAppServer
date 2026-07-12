namespace DeliveryManagementApp.Application.Customers.DTOs;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
