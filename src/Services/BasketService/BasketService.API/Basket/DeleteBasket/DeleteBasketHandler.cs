namespace BasketService.API.Basket.DeleteBasket;

internal class DeleteBasketHandler(IBasketRepository repository)
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
    {
        await repository.DeleteBasketAsync(command.UserName, cancellationToken);
        return new DeleteBasketResult(true);
    }
}


