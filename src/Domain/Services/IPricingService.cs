namespace DeliveryManagementApp.Domain.Services;

public interface IPricingService
{
    Task<decimal> CalculatePriceAsync(string originAddress, string destinationAddress, decimal weight, decimal volume, DateOnly requiredDate);
}
