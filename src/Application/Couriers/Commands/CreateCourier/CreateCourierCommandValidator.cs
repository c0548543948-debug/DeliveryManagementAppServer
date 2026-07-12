using FluentValidation;

namespace DeliveryManagementApp.Application.Couriers.Commands.CreateCourier;

public class CreateCourierCommandValidator : AbstractValidator<CreateCourierCommand>
{
    public CreateCourierCommandValidator()
    {
        RuleFor(x => x.ApplicationUserId).NotEmpty();
    }
}
