using DeliveryManagementApp.Domain.Services;

namespace DeliveryManagementApp.Application.Orders.Queries.CalculateOrderPrice;

public record CalculateOrderPriceQuery(
    string OriginAddress,
    string DestinationAddress,
    decimal Weight,
    decimal Volume,
    DateOnly RequiredDate) : IRequest<decimal>;

public class CalculateOrderPriceQueryHandler : IRequestHandler<CalculateOrderPriceQuery, decimal>
{
    private readonly IPricingService _pricingService;

    public CalculateOrderPriceQueryHandler(IPricingService pricingService)
        => _pricingService = pricingService;

    public Task<decimal> Handle(CalculateOrderPriceQuery request, CancellationToken cancellationToken)
        => _pricingService.CalculatePriceAsync(
            request.OriginAddress,
            request.DestinationAddress,
            request.Weight,
            request.Volume,
            request.RequiredDate);
}
