using FluentValidation;

namespace DeliveryManagementApp.Application.Routes.Commands.CreateRoute;

public class CreateRouteCommandValidator : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteCommandValidator()
    {
        RuleFor(x => x.CourierId).GreaterThan(0);
        RuleFor(x => x.Date).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime)
            .WithMessage("StartTime must be before EndTime.");
    }
}
