namespace BasketService.API.Basket.StoreBasket;

public class StoreBasketValidator : AbstractValidator<StoreBasketCommand>
{
    public StoreBasketValidator()
    {
        RuleFor(x => x.Cart)
            .NotNull().WithMessage("Cart is required.");

        RuleFor(x => x.Cart.UserName)
            .NotEmpty().WithMessage("UserName is required.");

        RuleFor(x => x.Cart.Items)
            .NotEmpty().WithMessage("Cart must contain at least one item.");
    }
}

