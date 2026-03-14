using BuildingBlocks.Exceptions;

namespace CatalogService.API.Exceptions;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(Guid id)
        : base("Product", id) { }
}