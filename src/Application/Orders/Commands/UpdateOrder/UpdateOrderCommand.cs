using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;

namespace DeliveryManagementApp.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(
    int Id,
    string OriginAddress,
    string DestinationAddress,
    decimal Weight,
    decimal Volume,
    DateOnly RequiredDate,
    decimal Price,
    OrderStatus Status) : IRequest;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateOrderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.FindAsync<Order>(request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, order);

        if (order.Status != request.Status)
        {
            _context.Add(new ActivityLog
            {
                OrderId = order.Id,
                OldStatus = order.Status,
                NewStatus = request.Status,
                Timestamp = DateTime.UtcNow,
                ChangedByUserId = _user.Id
            });
        }

        order.OriginAddress = request.OriginAddress;
        order.DestinationAddress = request.DestinationAddress;
        order.Weight = request.Weight;
        order.Volume = request.Volume;
        order.RequiredDate = request.RequiredDate;
        order.Price = request.Price;
        order.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
