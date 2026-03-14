using BuildingBlocks.CQRS;
using Mapster;
using Order.Application.DTOs;
using Order.Domain.Abstractions;

namespace Order.Application.Orders.Queries.GetOrdersByUserName;

internal class GetOrdersByUserNameHandler(IOrderRepository repository)
    : IQueryHandler<GetOrdersByUserNameQuery, GetOrdersByUserNameResult>
{
    public async Task<GetOrdersByUserNameResult> Handle(
        GetOrdersByUserNameQuery query, CancellationToken cancellationToken)
    {
        var orders = await repository.GetByUserNameAsync(query.UserName, cancellationToken);
        var dtos = orders.Adapt<IReadOnlyList<OrderDto>>();
        return new GetOrdersByUserNameResult(dtos);
    }
}
