using Carter;
using MediatR;
using Order.Application.Orders.Queries.GetOrderById;

namespace Order.API.Endpoints;

public class GetOrderByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/id/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetOrderByIdQuery(id));
                return Results.Ok(result);
            })
            .WithName("GetOrderById")
            .Produces<GetOrderByIdResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get order by ID")
            .WithDescription("Returns a single order by its ID.");
    }
}
