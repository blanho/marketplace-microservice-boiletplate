using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Order.Domain.Abstractions;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;

namespace Order.Application.Orders.Commands.CreateOrder;

internal class CreateOrderHandler(
    IOrderRepository repository,
    ILogger<CreateOrderHandler> logger)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var address = Address.Create(
            command.ShippingAddress.FirstName,
            command.ShippingAddress.LastName,
            command.ShippingAddress.EmailAddress,
            command.ShippingAddress.AddressLine,
            command.ShippingAddress.Country,
            command.ShippingAddress.State,
            command.ShippingAddress.ZipCode);

        var payment = Payment.Create(
            command.Payment.CardName,
            command.Payment.CardNumber,
            command.Payment.Expiration,
            command.Payment.Cvv,
            command.Payment.PaymentMethod);

        var items = command.Items.Select(i =>
            (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        var order = OrderAggregate.Create(command.UserName, address, payment, items);

        await repository.AddAsync(order, cancellationToken);

        logger.LogInformation("Order {OrderId} created for user {UserName}",
            order.Id, order.UserName);

        return new CreateOrderResult(order.Id);
    }
}
