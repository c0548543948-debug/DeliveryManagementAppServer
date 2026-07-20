using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Application.Orders.Commands.CreateOrder;
using DeliveryManagementApp.Application.Orders.Commands.DeleteOrder;
using DeliveryManagementApp.Application.Orders.Commands.UpdateOrder;
using DeliveryManagementApp.Application.Orders.Commands.UpdateOrderStatus;
using DeliveryManagementApp.Application.Orders.Queries.CalculateOrderPrice;
using DeliveryManagementApp.Application.Orders.Queries.GetOrderById;
using DeliveryManagementApp.Application.Orders.Queries.GetOrders;
using DeliveryManagementApp.Application.Orders.Queries.GetPendingOrders;
using DeliveryManagementApp.Application.Orders.Queries.ValidateAddress;
using DeliveryManagementApp.Domain.Enums;
using DeliveryManagementApp.Web.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementApp.Web.Endpoints;

public class Orders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.RequireAuthorization();
        group.MapGet(GetOrders);
        group.MapGet(GetOrderById, "{id}");
        group.MapGet(GetPendingOrders, "pending");
        group.MapGet(ValidateAddress, "validate-address");
        group.MapPost(CalculatePrice, "calculate-price");
        group.MapPost(CreateOrder);
        group.MapPut(UpdateOrder, "{id}");
        group.MapPatch(UpdateOrderStatus, "{id}/status");
        group.MapDelete(DeleteOrder, "{id}");
    }

    public static async Task<Ok<List<Application.Orders.DTOs.OrderDto>>> GetOrders(
        ISender sender, DateOnly? date = null, bool undelivered = false)
        => TypedResults.Ok(await sender.Send(new GetOrdersQuery(date, undelivered)));

    public static async Task<Ok<List<Application.Orders.DTOs.OrderDto>>> GetPendingOrders(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetPendingOrdersQuery()));

    public static async Task<Results<Ok<Application.Orders.DTOs.OrderDto>, NotFound>> GetOrderById(ISender sender, int id)
    {
        var result = await sender.Send(new GetOrderByIdQuery(id));
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Ok<AddressValidationResult>> ValidateAddress(ISender sender, [FromQuery] string address)
        => TypedResults.Ok(await sender.Send(new ValidateAddressQuery(address)));

    public static async Task<Ok<decimal>> CalculatePrice(ISender sender, CalculateOrderPriceQuery query)
        => TypedResults.Ok(await sender.Send(query));

    public static async Task<Created<int>> CreateOrder(ISender sender, CreateOrderCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Orders/{id}", id);
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateOrder(ISender sender, int id, UpdateOrderCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, BadRequest>> UpdateOrderStatus(
        ISender sender,
        IApplicationDbContext context,
        IRouteProgressService progress,
        IHubContext<TrackingHub> hub,
        int id,
        UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        await sender.Send(new UpdateOrderStatusCommand(id, request.Status));

        // Auto-start the route on first InTransit status if not already started
        if (request.Status == OrderStatus.InTransit)
        {
            var routeItemQuery = context.RouteItems.Where(i => i.OrderId == id);
            var routeItem = await context.ExecuteSingleAsync(routeItemQuery, ct);

            if (routeItem != null)
            {
                var routeQuery = context.Routes
                    .Include(r => r.Items)
                    .Where(r => r.Id == routeItem.RouteId);
                var route = await context.ExecuteSingleAsync(routeQuery, ct);

                if (route != null && !route.CurrentStop.HasValue)
                {
                    route.CurrentStop = 1;
                    await context.SaveChangesAsync(ct);
                    progress.StartRoute(route.Id, route.Items.Count);

                    var orderIds = route.Items.Select(i => i.OrderId).Distinct().ToList();
                    foreach (var oid in orderIds)
                        await hub.Clients.Group($"order-{oid}")
                            .SendAsync("ProgressUpdated", new { currentStop = 1, totalStops = route.Items.Count }, ct);
                }
                else if (route != null && !progress.GetProgress(route.Id).HasValue)
                {
                    // Route already started in DB but not in memory — warm the cache
                    progress.StartRoute(route.Id, route.Items.Count);
                    progress.SetCurrentStop(route.Id, route.CurrentStop!.Value);
                }
            }
        }

        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteOrder(ISender sender, int id)
    {
        await sender.Send(new DeleteOrderCommand(id));
        return TypedResults.NoContent();
    }
}

public record UpdateOrderStatusRequest(DeliveryManagementApp.Domain.Enums.OrderStatus Status);
