using BuildingBlocks.CQRS;
using Order.Application.DTOs;

namespace Order.Application.Orders.Queries.GetOrdersByUserName;

public record GetOrdersByUserNameQuery(string UserName) : IQuery<GetOrdersByUserNameResult>;

public record GetOrdersByUserNameResult(IReadOnlyList<OrderDto> Orders);
