using DeliveryManagementApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryManagementApp.Web.Hubs;

public class TrackingHub : Hub
{
    private readonly ITrackingTokenService _tokenService;

    public TrackingHub(ITrackingTokenService tokenService)
        => _tokenService = tokenService;

    // Customer joins tracking group using their token
    public async Task JoinTracking(string token)
    {
        var result = _tokenService.ValidateToken(token);
        if (result is null)
        {
            await Clients.Caller.SendAsync("Error", "Invalid or expired tracking token.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{result.Value.OrderId}");
        await Clients.Caller.SendAsync("Joined", $"Tracking order {result.Value.OrderId}");
    }

    // Courier pushes their location
    public async Task UpdateLocation(int orderId, double lat, double lng)
    {
        await Clients.Group($"order-{orderId}").SendAsync("LocationUpdated", new { lat, lng, timestamp = DateTime.UtcNow });
    }
}
