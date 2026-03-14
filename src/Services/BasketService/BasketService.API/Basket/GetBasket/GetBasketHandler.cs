namespace BasketService.API.Basket.GetBasket;

internal class GetBasketHandler(IBasketRepository repository)
    : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketAsync(query.UserName, cancellationToken);
        return new GetBasketResult(basket);
    }
}


