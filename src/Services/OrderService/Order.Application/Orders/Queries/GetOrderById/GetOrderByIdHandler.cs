using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Mapster;
using Order.Application.DTOs;
using Order.Domain.Abstractions;

namespace Order.Application.Orders.Queries.GetOrderById;

internal class GetOrderByIdHandler(IOrderRepository repository)
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(
        GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(query.Id, cancellationToken)
                    ?? throw new NotFoundException("Order", query.Id);

        return new GetOrderByIdResult(order.Adapt<OrderDto>());
    }
}
