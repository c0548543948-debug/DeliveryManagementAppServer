using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementApp.Application.Routes.Queries.GetRoutes;

public record GetRoutesQuery(DateOnly? Date = null, string? CourierUserId = null) : IRequest<List<RouteDto>>;

public class GetRoutesQueryHandler : IRequestHandler<GetRoutesQuery, List<RouteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUserNameService _userNames;

    public GetRoutesQueryHandler(IApplicationDbContext context, IMapper mapper, IUserNameService userNames)
    {
        _context = context;
        _mapper = mapper;
        _userNames = userNames;
    }

    public async Task<List<RouteDto>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Routes
            .Include(r => r.Items).ThenInclude(i => i.Order)
            .Include(r => r.Vehicle)
            .Include(r => r.Courier)
            .AsQueryable();

        if (request.Date.HasValue)
            query = query.Where(r => r.Date == request.Date.Value);

        if (!string.IsNullOrEmpty(request.CourierUserId))
            query = query.Where(r => r.Courier != null && r.Courier.ApplicationUserId == request.CourierUserId);

        var routes = await _context.ExecuteQueryAsync(query, cancellationToken);
        var dtos = _mapper.Map<List<RouteDto>>(routes);

        // Enrich with vehicle plate
        for (var i = 0; i < routes.Count; i++)
        {
            dtos[i].VehicleLicensePlate = routes[i].Vehicle?.LicensePlate ?? string.Empty;
        }

        // Enrich with courier names
        var courierUserIds = routes
            .Select(r => r.Courier?.ApplicationUserId)
            .Where(id => id != null)
            .Distinct()
            .Cast<string>();

        var names = await _userNames.GetNamesAsync(courierUserIds, cancellationToken);

        for (var i = 0; i < routes.Count; i++)
        {
            var userId = routes[i].Courier?.ApplicationUserId;
            if (userId != null && names.TryGetValue(userId, out var name))
                dtos[i].CourierName = $"{name.FirstName} {name.LastName}".Trim();
        }

        return dtos;
    }
}
