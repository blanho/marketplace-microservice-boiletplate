using FluentValidation;

namespace Order.Application.Orders.Commands.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.");

        RuleFor(x => x.ShippingAddress.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.ShippingAddress.EmailAddress)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Payment)
            .NotNull().WithMessage("Payment information is required.");

        RuleFor(x => x.Payment.CardNumber)
            .NotEmpty().WithMessage("Card number is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
