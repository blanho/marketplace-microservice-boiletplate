namespace BasketService.API.Entities;

public class ShoppingCartItem : Entity<Guid>
{
    public int Quantity { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
}