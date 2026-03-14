namespace Order.Application.DTOs;

public record OrderDto(
    Guid Id,
    string UserName,
    decimal TotalPrice,
    string Status,
    AddressDto ShippingAddress,
    PaymentDto Payment,
    List<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

public record AddressDto(
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode);

public record PaymentDto(
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
