using Discount.Grpc;

namespace BasketService.API.Basket.StoreBasket;

internal class StoreBasketHandler(
    IBasketRepository repository,
    DiscountService.DiscountServiceClient discountClient)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(
        StoreBasketCommand command, CancellationToken cancellationToken)
    {
        await ApplyDiscountsAsync(command.Cart, cancellationToken);

        var basket = await repository.UpdateBasketAsync(command.Cart, cancellationToken);
        return new StoreBasketResult(basket.UserName);
    }
    
    private async Task ApplyDiscountsAsync(
        ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            var coupon = await discountClient.GetDiscountAsync(
                new GetDiscountRequest { ProductName = item.ProductName },
                cancellationToken: cancellationToken);
            
            item.Price = Math.Max(0, item.Price - (decimal)coupon.Amount);
        }
    }
}



