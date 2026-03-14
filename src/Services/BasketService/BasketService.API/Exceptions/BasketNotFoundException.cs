namespace BasketService.API.Exceptions;

public class BasketNotFoundException : NotFoundException
{
    public BasketNotFoundException(string userName)
        : base("Basket", userName) { }
}

