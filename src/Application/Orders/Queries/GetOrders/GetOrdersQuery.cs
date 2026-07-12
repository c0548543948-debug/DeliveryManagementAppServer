using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.DTOs;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Orders.Queries.GetOrders;

/// <param name="Date">Filter by RequiredDate (optional).</param>
/// <param name="Undelivered">When true, only return orders with status &lt; Delivered (i.e. not Delivered or Cancelled).</param>
public record GetOrdersQuery(DateOnly? Date = null, bool Undelivered = false) : IRequest<List<OrderDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;

    public GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var ordersQuery = _context.Orders.AsQueryable();

        // Customers only see their own orders
        if (_user.Roles != null && _user.Roles.Contains("Customer"))
        {
            var customersQuery = _context.Customers.Where(c => c.ApplicationUserId == _user.Id);
            var customer = await _context.ExecuteSingleAsync(customersQuery, cancellationToken);
            ordersQuery = customer != null
                ? ordersQuery.Where(o => o.CustomerId == customer.Id)
                : ordersQuery.Where(_ => false);
        }

        // Optional date filter
        if (request.Date.HasValue)
            ordersQuery = ordersQuery.Where(o => o.RequiredDate == request.Date.Value);

        // Undelivered = Pending or Assigned (not yet Delivered / Cancelled)
        if (request.Undelivered)
            ordersQuery = ordersQuery.Where(o =>
                o.Status == OrderStatus.Pending || o.Status == OrderStatus.Assigned);

        var query = ordersQuery.ProjectTo<OrderDto>(_mapper.ConfigurationProvider);
        return await _context.ExecuteQueryAsync(query, cancellationToken);
    }
}
