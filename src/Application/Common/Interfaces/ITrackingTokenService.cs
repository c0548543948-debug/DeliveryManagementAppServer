namespace DeliveryManagementApp.Application.Common.Interfaces;

public interface ITrackingTokenService
{
    string GenerateToken(int orderId, int courierId);
    (int OrderId, int CourierId)? ValidateToken(string token);
}
