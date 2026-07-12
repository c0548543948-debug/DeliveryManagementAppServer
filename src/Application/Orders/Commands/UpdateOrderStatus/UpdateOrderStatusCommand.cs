using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(int Id, OrderStatus Status) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.FindAsync<Order>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, order);

        if (order.Status == request.Status) return;

        _context.Add(new ActivityLog
        {
            OrderId = order.Id,
            OldStatus = order.Status,
            NewStatus = request.Status,
            Timestamp = DateTime.UtcNow,
            ChangedByUserId = _user.Id
        });

        order.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
