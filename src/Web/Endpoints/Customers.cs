using DeliveryManagementApp.Application.Customers.Commands.CreateCustomer;
using DeliveryManagementApp.Application.Customers.Commands.DeleteCustomer;
using DeliveryManagementApp.Application.Customers.Commands.UpdateCustomer;
using DeliveryManagementApp.Application.Customers.Queries.GetCustomerById;
using DeliveryManagementApp.Application.Customers.Queries.GetCustomers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryManagementApp.Web.Endpoints;

public class Customers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetCustomers);
        group.MapGet(GetCustomerById, "{id}");
        group.MapPost(CreateCustomer);
        group.MapPut(UpdateCustomer, "{id}");
        group.MapDelete(DeleteCustomer, "{id}");
    }

    public static async Task<Ok<List<Application.Customers.DTOs.CustomerDto>>> GetCustomers(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetCustomersQuery()));

    public static async Task<Results<Ok<Application.Customers.DTOs.CustomerDto>, NotFound>> GetCustomerById(ISender sender, int id)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<int>> CreateCustomer(ISender sender, CreateCustomerCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Customers/{id}", id);
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateCustomer(ISender sender, int id, UpdateCustomerCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteCustomer(ISender sender, int id)
    {
        await sender.Send(new DeleteCustomerCommand(id));
        return TypedResults.NoContent();
    }
}
