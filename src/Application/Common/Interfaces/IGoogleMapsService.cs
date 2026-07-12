namespace DeliveryManagementApp.Application.Common.Interfaces;

public record GeoCoordinates(double Lat, double Lng);

public interface IGoogleMapsService
{
    Task<double?> GetDistanceInKmAsync(string origin, string destination);
    Task<bool> ValidateAddressAsync(string address);
    Task<GeoCoordinates?> GetCoordinatesAsync(string address);
}
