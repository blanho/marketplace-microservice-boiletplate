using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public class DiscountRepository(DiscountContext context, ILogger<DiscountRepository> logger)
    : IDiscountRepository
{
    // ── Read ─────────────────────────────────────────────────────────────────
    // AsNoTracking: no change-tracking overhead needed for gRPC read responses.
    public async Task<Coupon?> GetDiscountAsync(
        string productName, CancellationToken cancellationToken = default)
    {
        return await context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ProductName == productName, cancellationToken);
    }

    // ── Create ───────────────────────────────────────────────────────────────
    public async Task<Coupon> CreateDiscountAsync(
        Coupon coupon, CancellationToken cancellationToken = default)
    {
        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Discount created — id: {Id}, product: {ProductName}",
            coupon.Id, coupon.ProductName);

        return coupon;
    }

    // ── Update ───────────────────────────────────────────────────────────────
    // FindAsync: checks the identity map (first-level cache) before hitting the DB.
    public async Task<Coupon> UpdateDiscountAsync(
        Coupon coupon, CancellationToken cancellationToken = default)
    {
        var existing = await context.Coupons.FindAsync([coupon.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Coupon with Id {coupon.Id} was not found.");

        existing.ProductName = coupon.ProductName;
        existing.Description = coupon.Description;
        existing.Amount      = coupon.Amount;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Discount updated — id: {Id}, product: {ProductName}",
            existing.Id, existing.ProductName);

        return existing;
    }

    // ── Delete ───────────────────────────────────────────────────────────────
    public async Task<bool> DeleteDiscountAsync(
        string productName, CancellationToken cancellationToken = default)
    {
        var coupon = await context.Coupons
            .FirstOrDefaultAsync(c => c.ProductName == productName, cancellationToken);

        if (coupon is null) return false;

        context.Coupons.Remove(coupon);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Discount deleted — product: {ProductName}", productName);
        return true;
    }
}

