using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Infrastructure.Services;

public class PricingService : Domain.Services.IPricingService
{
    private const decimal BasePricePerKm = 2.0m;
    private const decimal PricePerKg = 1.5m;
    private const decimal PricePerCubicMeter = 3.0m;
    private const decimal UrgentSurchargeRate = 0.5m;
    private const decimal MinimumPrice = 20.0m;
    private const decimal FallbackKm = 10.0m;

    private readonly IGoogleMapsService _googleMapsService;

    public PricingService(IGoogleMapsService googleMapsService)
        => _googleMapsService = googleMapsService;

    public async Task<decimal> CalculatePriceAsync(string originAddress, string destinationAddress, decimal weight, decimal volume, DateOnly requiredDate)
    {
        var distanceKm = await _googleMapsService.GetDistanceInKmAsync(originAddress, destinationAddress);
        var km = (decimal)(distanceKm ?? (double)FallbackKm);

        var distancePrice = km * BasePricePerKm;
        var weightPrice = weight * PricePerKg;
        var volumePrice = volume * PricePerCubicMeter;

        var baseTotal = distancePrice + weightPrice + volumePrice;

        var daysUntilRequired = (requiredDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays;
        if (daysUntilRequired < 3)
            baseTotal += baseTotal * UrgentSurchargeRate;

        return Math.Max(Math.Round(baseTotal, 2), MinimumPrice);
    }
}
