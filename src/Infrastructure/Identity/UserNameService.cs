using DeliveryManagementApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementApp.Infrastructure.Identity;

public class UserNameService : IUserNameService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserNameService(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Dictionary<string, (string FirstName, string LastName)>> GetNamesAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToHashSet();
        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(u => u.Id, u => (u.FirstName, u.LastName));
    }
}
