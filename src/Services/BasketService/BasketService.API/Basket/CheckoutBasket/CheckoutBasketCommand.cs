using EventBus.Events.Integration;

namespace BasketService.API.Basket.CheckoutBasket;

public record CheckoutBasketCommand(BasketCheckoutDto CheckoutDto) : ICommand<CheckoutBasketResult>;

public record CheckoutBasketResult(bool IsSuccess);

public record BasketCheckoutDto(
    string UserName,
    // Shipping
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    // Payment
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod);
