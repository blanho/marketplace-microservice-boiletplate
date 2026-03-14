namespace Order.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5
}
