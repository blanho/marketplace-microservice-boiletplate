using BuildingBlocks.CQRS;
using Order.Application.DTOs;

namespace Order.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string UserName,
    AddressDto ShippingAddress,
    PaymentDto Payment,
    List<OrderItemDto> Items) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Guid OrderId);
