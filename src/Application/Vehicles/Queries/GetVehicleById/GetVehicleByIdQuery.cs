using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Vehicles.DTOs;

namespace DeliveryManagementApp.Application.Vehicles.Queries.GetVehicleById;

public record GetVehicleByIdQuery(int Id) : IRequest<VehicleDto?>;

public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVehicleByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<VehicleDto?> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vehicles
            .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
            .Where(v => v.VehicleId == request.Id);

        return await _context.ExecuteSingleAsync(query, cancellationToken);
    }
}
