using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;

namespace DeliveryManagementApp.Application.Routes.Queries.GetRouteById;

public record GetRouteByIdQuery(int Id) : IRequest<RouteDto?>;

public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, RouteDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRouteByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Routes
            .ProjectTo<RouteDto>(_mapper.ConfigurationProvider)
            .Where(r => r.RouteId.Equals(request.Id));

        return await _context.ExecuteSingleAsync(query, cancellationToken);
    }
}
