using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Routes.DTOs;
using DeliveryManagementApp.Domain.Constants;
using DeliveryManagementApp.Domain.Enums;
using DeliveryManagementApp.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryManagementApp.Web.Endpoints;

public class Tracking : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(GetTrackingToken, "{orderId}").RequireAuthorization();
        group.MapPost(PushLocation, "{orderId}/location")
             .RequireAuthorization(policy => policy.RequireRole(Roles.Courier));
        group.MapGet(GetOrderProgress, "order/{orderId}/progress").RequireAuthorization();
    }

    public static async Task<Ok<string>> GetTrackingToken(
        ITrackingTokenService tokenService,
        IApplicationDbContext context,
        IUser currentUser,
        int orderId)
    {
        // Look up the route item to find the real courierId
        var routeItemQuery = context.RouteItems.Where(ri => ri.OrderId == orderId);
        var routeItem = await context.ExecuteSingleAsync(routeItemQuery, default);

        int courierId = 0;
        if (routeItem != null)
        {
            var routeQuery = context.Routes.Where(r => r.Id == routeItem.RouteId);
            var route = await context.ExecuteSingleAsync(routeQuery, default);
            courierId = route?.CourierId ?? 0;
        }

        var token = tokenService.GenerateToken(orderId, courierId);
        return TypedResults.Ok(token);
    }

    public static async Task<Ok<OrderProgressDto>> GetOrderProgress(
        IApplicationDbContext context,
        IRouteProgressService progress,
        int orderId,
        CancellationToken ct)
    {
        var itemsQuery = context.RouteItems.Where(i => i.OrderId == orderId);
        var items = await context.ExecuteQueryAsync(itemsQuery, ct);

        int? routeId = items.FirstOrDefault()?.RouteId;
        int? pickupStop = items.FirstOrDefault(i => i.StopType == StopType.Pickup)?.StopOrder;
        int? deliveryStop = items.FirstOrDefault(i => i.StopType == StopType.Delivery)?.StopOrder;

        var p = routeId.HasValue ? progress.GetProgress(routeId.Value) : null;

        return TypedResults.Ok(new OrderProgressDto
        {
            PickupStop = pickupStop,
            DeliveryStop = deliveryStop,
            CurrentStop = p?.CurrentStop,
            TotalStops = p?.TotalStops,
            RouteId = routeId
        });
    }

    public static async Task<NoContent> PushLocation(
        IHubContext<TrackingHub> hubContext,
        int orderId,
        double lat,
        double lng)
    {
        await hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("LocationUpdated", new { lat, lng, timestamp = DateTime.UtcNow });
        return TypedResults.NoContent();
    }
}
