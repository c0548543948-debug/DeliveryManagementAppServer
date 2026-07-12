using FluentValidation;

namespace DeliveryManagementApp.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OriginAddress).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DestinationAddress).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Weight).GreaterThan(0);
        RuleFor(x => x.Volume).GreaterThan(0);
        RuleFor(x => x.RequiredDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
    }
}
