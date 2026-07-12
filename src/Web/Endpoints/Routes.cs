using DeliveryManagementApp.Application.Routes.Commands.BatchOptimizeRoutes;
using DeliveryManagementApp.Application.Routes.Commands.CreateOptimizedRoute;
using DeliveryManagementApp.Application.Routes.Commands.CreateRoute;
using DeliveryManagementApp.Application.Routes.Commands.DeleteRoute;
using DeliveryManagementApp.Application.Routes.Commands.OptimizeRoute;
using DeliveryManagementApp.Application.Routes.Commands.OptimizeRoutes;
using DeliveryManagementApp.Application.Routes.Commands.ReoptimizeRoute;
using DeliveryManagementApp.Application.Routes.Commands.UpdateRoute;
using DeliveryManagementApp.Application.Routes.Queries.GetRouteById;
using DeliveryManagementApp.Application.Routes.Queries.GetRouteMapUrl;
using DeliveryManagementApp.Application.Routes.Queries.GetRoutes;
using DeliveryManagementApp.Application.Routes.Queries.GetRoutesByDate;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryManagementApp.Web.Endpoints;

public class Routes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetRoutes);
        group.MapGet(GetRouteById, "{id}");
        group.MapGet(GetRouteMapUrl, "{id}/map");
        group.MapGet(GetRoutesByDate, "date/{date}");
        group.MapPost(OptimizeRoutes, "optimize");
        group.MapPost(BatchOptimizeRoutes, "batch-optimize");
        group.MapPost(OptimizeRoute, "single-optimize");
        group.MapPost(CreateOptimizedRoute, "create-optimized");
        group.MapPatch(ReoptimizeRoute, "{id}/reoptimize");
        group.MapPost(CreateRoute);
        group.MapPut(UpdateRoute, "{id}");
        group.MapDelete(DeleteRoute, "{id}");
    }

    public static async Task<Ok<List<Application.Routes.DTOs.RouteDto>>> GetRoutes(ISender sender, DateOnly? date = null)
        => TypedResults.Ok(await sender.Send(new GetRoutesQuery(date)));

    public static async Task<Ok<List<Application.Routes.DTOs.RouteDto>>> GetRoutesByDate(ISender sender, DateOnly date)
        => TypedResults.Ok(await sender.Send(new GetRoutesByDateQuery(date)));

    public static async Task<Results<Ok<Application.Routes.DTOs.RouteDto>, NotFound>> GetRouteById(ISender sender, int id)
    {
        var result = await sender.Send(new GetRouteByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<RouteMapDto>, NotFound>> GetRouteMapUrl(ISender sender, int id)
    {
        var result = await sender.Send(new GetRouteMapUrlQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<int>> OptimizeRoute(ISender sender, OptimizeRouteCommand command)
        => TypedResults.Ok(await sender.Send(command));

    public static async Task<Ok<List<int>>> BatchOptimizeRoutes(ISender sender, BatchOptimizeRoutesCommand command)
        => TypedResults.Ok(await sender.Send(command));

    public static async Task<Ok<int>> ReoptimizeRoute(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new ReoptimizeRouteCommand(id)));

    public static async Task<Ok<List<int>>> OptimizeRoutes(ISender sender, OptimizeRoutesCommand command)
        => TypedResults.Ok(await sender.Send(command));

    public static async Task<Created<int>> CreateRoute(ISender sender, CreateRouteCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Routes/{id}", id);
    }

    public static async Task<Results<Created<int>, BadRequest<string>>> CreateOptimizedRoute(
        ISender sender, CreateOptimizedRouteCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Created($"/api/Routes/{id}", id);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateRoute(ISender sender, int id, UpdateRouteCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteRoute(ISender sender, int id)
    {
        await sender.Send(new DeleteRouteCommand(id));
        return TypedResults.NoContent();
    }
}
