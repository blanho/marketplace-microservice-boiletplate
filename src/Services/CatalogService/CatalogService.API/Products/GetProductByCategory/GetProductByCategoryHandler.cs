namespace CatalogService.API.Products.GetProductByCategory;

internal class GetProductByCategoryHandler(IDocumentSession session)
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
{
    public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Product>()
            .Where(p => p.Category.Contains(query.Category))
            .ToListAsync(cancellationToken);

        if (!products.Any())
            throw new NotFoundException($"No products found in category \"{query.Category}\".");

        return new GetProductByCategoryResult(products);
    }
}

