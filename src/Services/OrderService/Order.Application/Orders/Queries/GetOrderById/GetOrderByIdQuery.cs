using BuildingBlocks.CQRS;
using Order.Application.DTOs;

namespace Order.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IQuery<GetOrderByIdResult>;

public record GetOrderByIdResult(OrderDto Order);
