using DeliveryManagementApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryManagementApp.Web.Endpoints;

public class Tracking : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(GetTrackingToken, "{orderId}").RequireAuthorization();
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
}
