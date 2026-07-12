using DeliveryManagementApp.Application.Orders.Commands.CreateOrder;
using DeliveryManagementApp.Application.Orders.Commands.DeleteOrder;
using DeliveryManagementApp.Application.Orders.Commands.UpdateOrder;
using DeliveryManagementApp.Application.Orders.Commands.UpdateOrderStatus;
using DeliveryManagementApp.Application.Orders.Queries.CalculateOrderPrice;
using DeliveryManagementApp.Application.Orders.Queries.GetOrderById;
using DeliveryManagementApp.Application.Orders.Queries.GetOrders;
using DeliveryManagementApp.Application.Orders.Queries.GetPendingOrders;
using DeliveryManagementApp.Application.Orders.Queries.ValidateAddress;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

    public static async Task<Results<NoContent, BadRequest>> UpdateOrderStatus(ISender sender, int id, UpdateOrderStatusRequest request)
    {
        await sender.Send(new UpdateOrderStatusCommand(id, request.Status));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DeleteOrder(ISender sender, int id)
    {
        await sender.Send(new DeleteOrderCommand(id));
        return TypedResults.NoContent();
    }
}

public record UpdateOrderStatusRequest(DeliveryManagementApp.Domain.Enums.OrderStatus Status);
