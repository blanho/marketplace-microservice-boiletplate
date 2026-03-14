using SharedKernel;

namespace Order.Domain.ValueObjects;

public class Payment : ValueObject
{
    public string CardName { get; } = default!;
    public string CardNumber { get; } = default!;
    public string Expiration { get; } = default!;
    public string Cvv { get; } = default!;
    public int PaymentMethod { get; }

    protected Payment() { }

    private Payment(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Create(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);

        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CardName;
        yield return CardNumber;
        yield return Expiration;
        yield return Cvv;
        yield return PaymentMethod;
    }
}
