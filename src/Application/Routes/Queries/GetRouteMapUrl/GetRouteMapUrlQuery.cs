using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Routes.Queries.GetRouteMapUrl;

public record RouteStopInfo(string Address, TimeOnly? EstimatedArrival, double? Lat, double? Lng);
public record RouteMapDto(string NavigationUrl, List<RouteStopInfo> Stops);

public record GetRouteMapUrlQuery(int RouteId) : IRequest<RouteMapDto?>;

public class GetRouteMapUrlQueryHandler : IRequestHandler<GetRouteMapUrlQuery, RouteMapDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IGoogleMapsService _googleMapsService;

    public GetRouteMapUrlQueryHandler(IApplicationDbContext context, IGoogleMapsService googleMapsService)
    {
        _context = context;
        _googleMapsService = googleMapsService;
    }

    public async Task<RouteMapDto?> Handle(GetRouteMapUrlQuery request, CancellationToken cancellationToken)
    {
        var route = await _context.GetRouteWithItemsAsync(request.RouteId, cancellationToken);

        if (route is null) return null;

        var orderedStops = route.Items.OrderBy(i => i.StopOrder).ToList();
        if (orderedStops.Count == 0) return null;

        // Get coordinates and addresses for each ordered stop, using pickup->origin and delivery->destination
        var stops = new List<RouteStopInfo>();
        var addressesOrdered = new List<string>();
        foreach (var item in orderedStops)
        {
            var address = item.StopType == StopType.Pickup
                ? item.Order.OriginAddress
                : item.Order.DestinationAddress;

            var coords = await _googleMapsService.GetCoordinatesAsync(address);
            stops.Add(new RouteStopInfo(
                address,
                item.EstimatedArrival,
                coords?.Lat,
                coords?.Lng));

            addressesOrdered.Add(Uri.EscapeDataString(address));
        }

        // Build Google Maps navigation URL from the ordered addresses
        var origin = addressesOrdered.First();
        var destination = addressesOrdered.Last();
        var waypoints = addressesOrdered.Count > 2
            ? string.Join("/", addressesOrdered.Skip(1).Take(addressesOrdered.Count - 2))
            : string.Empty;

        var navigationUrl = !string.IsNullOrEmpty(waypoints)
            ? $"https://www.google.com/maps/dir/{origin}/{waypoints}/{destination}"
            : $"https://www.google.com/maps/dir/{origin}/{destination}";

        return new RouteMapDto(navigationUrl, stops);
    }
}
