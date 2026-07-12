using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int Id) : IRequest;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.FindAsync<DeliveryManagementApp.Domain.Entities.Customer>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, customer);

        _context.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
