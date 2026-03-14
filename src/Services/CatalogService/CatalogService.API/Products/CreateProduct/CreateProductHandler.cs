namespace CatalogService.API.Products.CreateProduct;

internal class CreateProductHandler(IDocumentSession session)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id          = Guid.NewGuid(),
            Name        = command.Name,
            Description = command.Description,
            Price       = command.Price,
        };
        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);
        return new CreateProductResult(product.Id);
    }
}

