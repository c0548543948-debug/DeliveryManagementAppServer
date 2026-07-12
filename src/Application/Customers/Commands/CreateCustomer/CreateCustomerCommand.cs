using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(string ApplicationUserId, string Phone) : IRequest<int>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer { ApplicationUserId = request.ApplicationUserId, Phone = request.Phone };
        _context.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }
}
