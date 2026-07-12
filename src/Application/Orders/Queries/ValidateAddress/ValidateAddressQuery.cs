using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Orders.Queries.ValidateAddress;

public record AddressValidationResult(bool IsValid, string FormattedAddress, double Lat, double Lng);

public record ValidateAddressQuery(string Address) : IRequest<AddressValidationResult>;

public class ValidateAddressQueryHandler : IRequestHandler<ValidateAddressQuery, AddressValidationResult>
{
    private readonly IGoogleMapsService _googleMapsService;

    public ValidateAddressQueryHandler(IGoogleMapsService googleMapsService)
        => _googleMapsService = googleMapsService;

    public async Task<AddressValidationResult> Handle(ValidateAddressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _googleMapsService.ValidateAddressAsync(request.Address);
            if (!isValid)
                return new AddressValidationResult(false, string.Empty, 0, 0);

            var coords = await _googleMapsService.GetCoordinatesAsync(request.Address);
            return new AddressValidationResult(true, request.Address, coords?.Lat ?? 0, coords?.Lng ?? 0);
        }
        catch
        {
            // If Google Maps API is unavailable, accept the address as-is
            return new AddressValidationResult(true, request.Address, 0, 0);
        }
    }
}
