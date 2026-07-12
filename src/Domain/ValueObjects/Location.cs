namespace DeliveryManagementApp.Domain.ValueObjects;

public class Location(double? latitude, double? longitude) : ValueObject
{
    public double? Latitude { get; private set; } = latitude;

    public double? Longitude { get; private set; } = longitude;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude ?? 0.0;
        yield return Longitude ?? 0.0;
    }

    public override string ToString()
    {
        return Latitude.HasValue && Longitude.HasValue ? $"{Latitude.Value},{Longitude.Value}" : string.Empty;
    }
}
