using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Enums;
using DeliveryManagementApp.Domain.Services;

namespace DeliveryManagementApp.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string OriginAddress,
    string DestinationAddress,
    decimal Weight,
    decimal Volume,
    DateOnly RequiredDate,
    TimeOnly? TimeWindowStart = null,
    TimeOnly? TimeWindowEnd = null) : IRequest<int>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPricingService _pricingService;
    private readonly IUser _currentUser;

    public CreateOrderCommandHandler(IApplicationDbContext context, IPricingService pricingService, IUser currentUser)
    {
        _context = context;
        _pricingService = pricingService;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Resolve CustomerId from the logged-in user's identity
        var applicationUserId = _currentUser.Id
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var customer = await _context.ExecuteSingleAsync(
            _context.Customers.Where(c => c.ApplicationUserId == applicationUserId),
            cancellationToken)
            ?? throw new InvalidOperationException("Customer profile not found for the current user.");

        var price = await _pricingService.CalculatePriceAsync(
            request.OriginAddress,
            request.DestinationAddress,
            request.Weight,
            request.Volume,
            request.RequiredDate);

        var order = new Order
        {
            CustomerId = customer.Id,
            OriginAddress = request.OriginAddress,
            DestinationAddress = request.DestinationAddress,
            Weight = request.Weight,
            Volume = request.Volume,
            RequiredDate = request.RequiredDate,
            Price = price,
            Status = OrderStatus.Pending,
            TimeWindowStart = request.TimeWindowStart,
            TimeWindowEnd = request.TimeWindowEnd
        };

        _context.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
