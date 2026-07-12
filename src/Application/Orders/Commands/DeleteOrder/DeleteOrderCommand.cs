using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(int Id) : IRequest;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.FindAsync<DeliveryManagementApp.Domain.Entities.Order>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, order);

        _context.Remove<DeliveryManagementApp.Domain.Entities.Order>(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
