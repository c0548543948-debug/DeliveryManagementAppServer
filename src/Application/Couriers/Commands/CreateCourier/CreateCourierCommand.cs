using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;

namespace DeliveryManagementApp.Application.Couriers.Commands.CreateCourier;

public record CreateCourierCommand(string ApplicationUserId) : IRequest<int>;

public class CreateCourierCommandHandler : IRequestHandler<CreateCourierCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCourierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = new Courier { ApplicationUserId = request.ApplicationUserId };
        _context.Add(courier);
        await _context.SaveChangesAsync(cancellationToken);
        return courier.Id;
    }
}
