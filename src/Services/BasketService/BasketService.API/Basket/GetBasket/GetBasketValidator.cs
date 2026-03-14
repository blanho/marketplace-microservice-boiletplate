namespace BasketService.API.Basket.GetBasket;

public class GetBasketValidator : AbstractValidator<GetBasketQuery>
{
    public GetBasketValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.");
    }
}

