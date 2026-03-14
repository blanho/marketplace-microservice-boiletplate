using Order.Domain.Entities;

namespace Order.Domain.Abstractions;

public interface IOrderRepository
{
    Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderAggregate>> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<OrderAggregate> AddAsync(OrderAggregate order, CancellationToken cancellationToken = default);
    void Update(OrderAggregate order);
}
