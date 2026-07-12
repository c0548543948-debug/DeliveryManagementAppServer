using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Customers.DTOs;

namespace DeliveryManagementApp.Application.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .Where(c => c.CustomerId == request.Id);

        return await _context.ExecuteSingleAsync(query, cancellationToken);
    }
}
