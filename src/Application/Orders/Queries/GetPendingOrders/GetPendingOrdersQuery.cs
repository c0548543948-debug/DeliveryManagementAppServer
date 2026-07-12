using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.DTOs;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Orders.Queries.GetPendingOrders;

public record GetPendingOrdersQuery : IRequest<List<OrderDto>>;

public class GetPendingOrdersQueryHandler : IRequestHandler<GetPendingOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPendingOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider);

        return await _context.ExecuteQueryAsync(query, cancellationToken);
    }
}
