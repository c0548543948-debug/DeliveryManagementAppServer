namespace DeliveryManagementApp.Application.Common.Interfaces;

/// <summary>Resolves display names for application users by their Identity user IDs.</summary>
public interface IUserNameService
{
    Task<Dictionary<string, (string FirstName, string LastName, string Email)>> GetNamesAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
}
