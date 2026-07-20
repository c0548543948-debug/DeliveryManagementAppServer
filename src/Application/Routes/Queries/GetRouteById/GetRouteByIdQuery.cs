using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementApp.Application.Routes.Queries.GetRouteById;

public record GetRouteByIdQuery(int Id) : IRequest<RouteDto?>;

public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, RouteDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserNameService _userNames;

    public GetRouteByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IUserNameService userNames)
    {
        _context = context;
        _mapper = mapper;
        _userNames = userNames;
    }

    public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Routes
            .Include(r => r.Items).ThenInclude(i => i.Order)
            .Include(r => r.Vehicle)
            .Include(r => r.Courier)
            .Where(r => r.Id == request.Id);

        var route = await _context.ExecuteSingleAsync(query, cancellationToken);
        if (route is null) return null;

        var dto = _mapper.Map<RouteDto>(route);
        dto.VehicleLicensePlate = route.Vehicle?.LicensePlate ?? string.Empty;

        var userId = route.Courier?.ApplicationUserId;
        if (userId != null)
        {
            var names = await _userNames.GetNamesAsync([userId], cancellationToken);
            if (names.TryGetValue(userId, out var name))
                dto.CourierName = $"{name.FirstName} {name.LastName}".Trim();
        }

        return dto;
    }
}
