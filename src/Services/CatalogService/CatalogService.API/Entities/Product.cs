using SharedKernel;

namespace CatalogService.API.Entities;

public class Product : Entity<Guid>
{
    public required string Name { get; set; }
    public List<string> Category { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string ImageFile { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

