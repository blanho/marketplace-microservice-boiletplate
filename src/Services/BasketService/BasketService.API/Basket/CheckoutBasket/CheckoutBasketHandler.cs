using EventBus.Events.Integration;
using MassTransit;

namespace BasketService.API.Basket.CheckoutBasket;

internal class CheckoutBasketHandler(
    IBasketRepository repository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(
        CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var dto = command.CheckoutDto;

        // 1. Get the basket for the user
        var basket = await repository.GetBasketAsync(dto.UserName, cancellationToken);

        // 2. Build the integration event
        var checkoutEvent = new BasketCheckoutEvent
        {
            UserName = dto.UserName,
            TotalPrice = basket.TotalPrice,
            Items = basket.Items.Select(i => new BasketCheckoutItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList(),

            // Shipping
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailAddress = dto.EmailAddress,
            AddressLine = dto.AddressLine,
            Country = dto.Country,
            State = dto.State,
            ZipCode = dto.ZipCode,

            // Payment
            CardName = dto.CardName,
            CardNumber = dto.CardNumber,
            Expiration = dto.Expiration,
            Cvv = dto.Cvv,
            PaymentMethod = dto.PaymentMethod
        };

        // 3. Publish to RabbitMQ — OrderService will consume this
        await publishEndpoint.Publish(checkoutEvent, cancellationToken);

        // 4. Delete the basket after successful checkout
        await repository.DeleteBasketAsync(dto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}
