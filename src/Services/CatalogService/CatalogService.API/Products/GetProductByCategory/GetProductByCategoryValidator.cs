namespace CatalogService.API.Products.GetProductByCategory;

public class GetProductByCategoryValidator : AbstractValidator<GetProductByCategoryQuery>
{
    public GetProductByCategoryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters.");
    }
}

