namespace BasketService.API.Data;

public interface IBasketRepository
{
    Task<ShoppingCart> GetBasketAsync(string userName, CancellationToken cancellationToken = default);
    Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default);
}