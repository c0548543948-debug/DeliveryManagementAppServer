using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Couriers.DTOs;

namespace DeliveryManagementApp.Application.Couriers.Queries.GetCourierById;

public record GetCourierByIdQuery(int Id) : IRequest<CourierDto?>;

public class GetCourierByIdQueryHandler : IRequestHandler<GetCourierByIdQuery, CourierDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCourierByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CourierDto?> Handle(GetCourierByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Couriers
            .ProjectTo<CourierDto>(_mapper.ConfigurationProvider)
            .Where(c => c.CourierId == request.Id);

        return await _context.ExecuteSingleAsync(query, cancellationToken);
    }
}
