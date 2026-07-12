using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(int Id, string Phone) : IRequest;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.FindAsync<DeliveryManagementApp.Domain.Entities.Customer>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, customer);

        customer.Phone = request.Phone;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
