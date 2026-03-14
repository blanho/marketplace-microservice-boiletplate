using Carter;
using MediatR;
using Order.Application.Orders.Queries.GetOrdersByUserName;

namespace Order.API.Endpoints;

public class GetOrdersByUserNameEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{userName}", async (string userName, ISender sender) =>
            {
                var result = await sender.Send(new GetOrdersByUserNameQuery(userName));
                return Results.Ok(result);
            })
            .WithName("GetOrdersByUserName")
            .Produces<GetOrdersByUserNameResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get orders by user name")
            .WithDescription("Returns all orders for the specified user.");
    }
}
