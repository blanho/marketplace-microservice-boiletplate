using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace BasketService.API.Data;

public class CacheBasketRepository(
    IBasketRepository repository,
    IDistributedCache cache) : IBasketRepository
{
    private static string CacheKey(string userName) => $"basket:{userName}";

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
        SlidingExpiration               = TimeSpan.FromMinutes(30)
    };

    public async Task<ShoppingCart> GetBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetStringAsync(CacheKey(userName), cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize<ShoppingCart>(cached)!;
        
        var basket = await repository.GetBasketAsync(userName, cancellationToken);
        
        await cache.SetStringAsync(
            CacheKey(userName),
            JsonSerializer.Serialize(basket),
            CacheOptions,
            cancellationToken);

        return basket;
    }

    public async Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        var updated = await repository.UpdateBasketAsync(basket, cancellationToken);
        
        await cache.SetStringAsync(
            CacheKey(updated.UserName),
            JsonSerializer.Serialize(updated),
            CacheOptions,
            cancellationToken);

        return updated;
    }

    public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        var result = await repository.DeleteBasketAsync(userName, cancellationToken);
        
        await cache.RemoveAsync(CacheKey(userName), cancellationToken);

        return result;
    }
}