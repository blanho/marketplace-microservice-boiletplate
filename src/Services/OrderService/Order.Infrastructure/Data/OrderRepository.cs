using Microsoft.EntityFrameworkCore;
using Order.Domain.Abstractions;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data;

public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    public async Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderAggregate>> GetByUserNameAsync(
        string userName, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserName == userName)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderAggregate> AddAsync(
        OrderAggregate order, CancellationToken cancellationToken = default)
    {
        await context.Orders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public void Update(OrderAggregate order)
    {
        context.Orders.Update(order);
    }
}
