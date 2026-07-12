using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Vehicles.DTOs;

namespace DeliveryManagementApp.Application.Vehicles.Queries.GetVehicles;

public record GetVehiclesQuery : IRequest<List<VehicleDto>>;

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, List<VehicleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVehiclesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<VehicleDto>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vehicles
            .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider);

        return await _context.ExecuteQueryAsync(query, cancellationToken);
    }
}
