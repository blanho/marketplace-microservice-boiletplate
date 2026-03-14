namespace CatalogService.API.Products.GetProductById;

internal class GetProductByIdHandler(IDocumentSession session)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(query.Id, cancellationToken)
            ?? throw new ProductNotFoundException(query.Id);

        return new GetProductByIdResult(product);
    }
}

