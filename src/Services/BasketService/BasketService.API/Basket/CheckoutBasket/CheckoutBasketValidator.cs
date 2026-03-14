namespace BasketService.API.Basket.CheckoutBasket;

public class CheckoutBasketValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketValidator()
    {
        RuleFor(x => x.CheckoutDto).NotNull();
        RuleFor(x => x.CheckoutDto.UserName).NotEmpty().WithMessage("UserName is required.");
        RuleFor(x => x.CheckoutDto.FirstName).NotEmpty().WithMessage("FirstName is required.");
        RuleFor(x => x.CheckoutDto.LastName).NotEmpty().WithMessage("LastName is required.");
        RuleFor(x => x.CheckoutDto.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(x => x.CheckoutDto.CardNumber).NotEmpty().WithMessage("Card number is required.");
    }
}
