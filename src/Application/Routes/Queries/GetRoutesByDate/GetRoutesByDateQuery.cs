using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;

namespace DeliveryManagementApp.Application.Routes.Queries.GetRoutesByDate;

public record GetRoutesByDateQuery(DateOnly Date) : IRequest<List<RouteDto>>;

public class GetRoutesByDateQueryHandler : IRequestHandler<GetRoutesByDateQuery, List<RouteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRoutesByDateQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RouteDto>> Handle(GetRoutesByDateQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Routes
            .Where(r => r.Date == request.Date)
            .ProjectTo<RouteDto>(_mapper.ConfigurationProvider);

        return await _context.ExecuteQueryAsync(query, cancellationToken);
    }
}
