using FluentValidation;

namespace Order.Application.Orders.Queries.GetOrdersByUserName;

public class GetOrdersByUserNameValidator : AbstractValidator<GetOrdersByUserNameQuery>
{
    public GetOrdersByUserNameValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.");
    }
}
