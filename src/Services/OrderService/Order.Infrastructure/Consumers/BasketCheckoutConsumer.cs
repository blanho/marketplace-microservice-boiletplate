using EventBus.Events.Integration;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Application.Orders.Commands.CreateOrder;

namespace Order.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles BasketCheckoutEvent from RabbitMQ.
/// Translates the integration event into a CreateOrderCommand and dispatches via MediatR.
/// </summary>
public class BasketCheckoutConsumer(
    ISender sender,
    ILogger<BasketCheckoutConsumer> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Consuming BasketCheckoutEvent — User: {UserName}, Total: {TotalPrice}",
            message.UserName, message.TotalPrice);

        var command = new CreateOrderCommand(
            UserName: message.UserName,
            ShippingAddress: new AddressDto(
                message.FirstName, message.LastName, message.EmailAddress,
                message.AddressLine, message.Country, message.State, message.ZipCode),
            Payment: new PaymentDto(
                message.CardName, message.CardNumber, message.Expiration,
                message.Cvv, message.PaymentMethod),
            Items: message.Items.Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.Quantity, i.Price)).ToList());

        var result = await sender.Send(command, context.CancellationToken);

        logger.LogInformation(
            "Order {OrderId} created from BasketCheckout for user {UserName}",
            result.OrderId, message.UserName);
    }
}
