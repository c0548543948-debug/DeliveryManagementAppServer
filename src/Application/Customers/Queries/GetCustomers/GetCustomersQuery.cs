using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Customers.DTOs;

namespace DeliveryManagementApp.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IRequest<List<CustomerDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCustomersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider);

        return await _context.ExecuteQueryAsync(query, cancellationToken);
    }
}
