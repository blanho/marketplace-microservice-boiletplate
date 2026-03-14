using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;

// Alias to avoid naming conflict: our class and the generated class share the name "DiscountService".
using DiscountProtoService = global::Discount.Grpc.DiscountService;

namespace Discount.Grpc.Services;

public class DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountServiceBase
{
    public override async Task<CouponModel> GetDiscount(
        GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await repository.GetDiscountAsync(request.ProductName, context.CancellationToken)
                     ?? new Coupon { ProductName = "No Discount", Description = "No discount available.", Amount = 0 };

        logger.LogInformation(
            "Discount fetched — product: {ProductName}, amount: {Amount}",
            coupon.ProductName, coupon.Amount);

        return MapToModel(coupon);
    }

    public override async Task<CouponModel> CreateDiscount(
        CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = new Coupon
        {
            ProductName = request.Coupon.ProductName,
            Description = request.Coupon.Description,
            Amount      = request.Coupon.Amount
        };

        var created = await repository.CreateDiscountAsync(coupon, context.CancellationToken);

        logger.LogInformation(
            "Discount created — product: {ProductName}, id: {Id}",
            created.ProductName, created.Id);

        return MapToModel(created);
    }

    public override async Task<CouponModel> UpdateDiscount(
        UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = new Coupon
        {
            Id          = request.Coupon.Id,
            ProductName = request.Coupon.ProductName,
            Description = request.Coupon.Description,
            Amount      = request.Coupon.Amount
        };

        var updated = await repository.UpdateDiscountAsync(coupon, context.CancellationToken);

        logger.LogInformation(
            "Discount updated — product: {ProductName}, id: {Id}",
            updated.ProductName, updated.Id);

        return MapToModel(updated);
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(
        DeleteDiscountRequest request, ServerCallContext context)
    {
        var success = await repository.DeleteDiscountAsync(request.ProductName, context.CancellationToken);

        logger.LogInformation(
            "Discount deleted — product: {ProductName}, success: {Success}",
            request.ProductName, success);

        return new DeleteDiscountResponse { Success = success };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static CouponModel MapToModel(Coupon coupon) => new()
    {
        Id          = coupon.Id,
        ProductName = coupon.ProductName,
        Description = coupon.Description,
        Amount      = coupon.Amount
    };
}

