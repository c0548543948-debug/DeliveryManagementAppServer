using DeliveryManagementApp.Application.Vehicles.Commands.CreateVehicle;
using DeliveryManagementApp.Application.Vehicles.Commands.DeleteVehicle;
using DeliveryManagementApp.Application.Vehicles.Commands.UpdateVehicle;
using DeliveryManagementApp.Application.Vehicles.Queries.GetVehicleById;
using DeliveryManagementApp.Application.Vehicles.Queries.GetVehicles;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryManagementApp.Web.Endpoints;

public class Vehicles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetVehicles);
        group.MapGet(GetVehicleById, "{id}");
        group.MapPost(CreateVehicle);
        group.MapPut(UpdateVehicle, "{id}");
        group.MapDelete(DeleteVehicle, "{id}");
    }

    public static async Task<Ok<List<Application.Vehicles.DTOs.VehicleDto>>> GetVehicles(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetVehiclesQuery()));

    public static async Task<Results<Ok<Application.Vehicles.DTOs.VehicleDto>, NotFound>> GetVehicleById(ISender sender, int id)
    {
        var result = await sender.Send(new GetVehicleByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<int>> CreateVehicle(ISender sender, CreateVehicleCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Vehicles/{id}", id);
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateVehicle(ISender sender, int id, UpdateVehicleCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteVehicle(ISender sender, int id)
    {
        await sender.Send(new DeleteVehicleCommand(id));
        return TypedResults.NoContent();
    }
}
