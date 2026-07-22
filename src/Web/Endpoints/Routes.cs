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
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;
using DeliveryManagementApp.Domain.Constants;
using DeliveryManagementApp.Domain.Enums;
using DeliveryManagementApp.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
        group.MapPost(StartRoute, "{id}/start")
             .RequireAuthorization(policy => policy.RequireRole(Roles.Courier));
        group.MapPatch(SetCurrentStop, "{id}/current-stop")
             .RequireAuthorization(policy => policy.RequireRole(Roles.Courier));
    }

    public static async Task<Ok<List<Application.Routes.DTOs.RouteDto>>> GetRoutes(ISender sender, IUser currentUser, DateOnly? date = null)
    {
        // When a Courier calls this endpoint, filter to only their own routes
        string? courierUserId = currentUser.Roles?.Contains(Roles.Courier) == true ? currentUser.Id : null;
        return TypedResults.Ok(await sender.Send(new GetRoutesQuery(date, courierUserId)));
    }

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

    public static async Task<Results<NoContent, NotFound>> StartRoute(
        IApplicationDbContext context,
        IRouteProgressService progress,
        IHubContext<TrackingHub> hub,
        int id,
        CancellationToken ct)
    {
        var routeQuery = context.Routes
            .Include(r => r.Items).ThenInclude(i => i.Order)
            .Where(r => r.Id == id);
        var route = await context.ExecuteSingleAsync(routeQuery, ct);
        if (route is null) return TypedResults.NotFound();

        int totalStops = route.Items.Count;

        if (!route.CurrentStop.HasValue)
        {
            // First start: set all orders InTransit so customers can track, and set currentStop = 1
            foreach (var item in route.Items.Where(i => i.Order != null))
                item.Order!.Status = OrderStatus.InTransit;
            route.CurrentStop = 1;
            await context.SaveChangesAsync(ct);

            // Warm in-memory and broadcast
            progress.StartRoute(id, totalStops);
            var orderIds = route.Items.Select(i => i.OrderId).Distinct().ToList();
            foreach (var orderId in orderIds)
                await hub.Clients.Group($"order-{orderId}")
                    .SendAsync("ProgressUpdated", new { currentStop = 1, totalStops }, ct);
        }
        else
        {
            // Already started (e.g., server restart cleared in-memory) — restore in-memory only
            if (!progress.GetProgress(id).HasValue)
            {
                progress.StartRoute(id, totalStops);
                progress.SetCurrentStop(id, route.CurrentStop.Value);
            }
        }

        return TypedResults.NoContent();
    }

    public static async Task<NoContent> SetCurrentStop(
        IApplicationDbContext context,
        IRouteProgressService progress,
        IHubContext<TrackingHub> hub,
        int id,
        int stop,
        CancellationToken ct)
    {
        // Persist to DB
        var route = await context.ExecuteSingleAsync(context.Routes.Where(r => r.Id == id), ct);
        if (route is not null)
        {
            route.CurrentStop = stop;
            await context.SaveChangesAsync(ct);
        }

        // Mirror to in-memory
        progress.SetCurrentStop(id, stop);
        var p = progress.GetProgress(id);

        var itemsQuery = context.RouteItems.Where(i => i.RouteId == id);
        var items = await context.ExecuteQueryAsync(itemsQuery, ct);

        int totalStops = p?.TotalStops ?? items.Count;
        foreach (var item in items)
            await hub.Clients.Group($"order-{item.OrderId}")
                .SendAsync("ProgressUpdated", new { currentStop = stop, totalStops }, ct);

        return TypedResults.NoContent();
    }
}
