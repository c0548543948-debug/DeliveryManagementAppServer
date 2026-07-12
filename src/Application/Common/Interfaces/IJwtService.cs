namespace DeliveryManagementApp.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string role, string? firstName = null, string? lastName = null);
}
