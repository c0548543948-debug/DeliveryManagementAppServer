using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.DTOs;

namespace DeliveryManagementApp.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Orders
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                .Where(o => o.OrderId == request.Id);

            return await _context.ExecuteSingleAsync(query, cancellationToken);
        }
}
