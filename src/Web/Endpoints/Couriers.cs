using DeliveryManagementApp.Application.Couriers.Commands.CreateCourier;
using DeliveryManagementApp.Application.Couriers.Commands.DeleteCourier;
using DeliveryManagementApp.Application.Couriers.Commands.UpdateCourier;
using DeliveryManagementApp.Application.Couriers.Queries.GetCourierById;
using DeliveryManagementApp.Application.Couriers.Queries.GetCouriers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryManagementApp.Web.Endpoints;

public class Couriers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetCouriers);
        group.MapGet(GetCourierById, "{id}");
        group.MapPost(CreateCourier);
        group.MapPut(UpdateCourier, "{id}");
        group.MapDelete(DeleteCourier, "{id}");
    }

    public static async Task<Ok<List<Application.Couriers.DTOs.CourierDto>>> GetCouriers(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetCouriersQuery()));

    public static async Task<Results<Ok<Application.Couriers.DTOs.CourierDto>, NotFound>> GetCourierById(ISender sender, int id)
    {
        var result = await sender.Send(new GetCourierByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<int>> CreateCourier(ISender sender, CreateCourierCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Couriers/{id}", id);
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateCourier(ISender sender, int id, UpdateCourierCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteCourier(ISender sender, int id)
    {
        await sender.Send(new DeleteCourierCommand(id));
        return TypedResults.NoContent();
    }
}
