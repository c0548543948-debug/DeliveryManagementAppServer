using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DeliveryManagementApp.Application.Common.Interfaces;

namespace DeliveryManagementApp.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // מנסה כמה claim types כי .NET ממפה "sub" בצורות שונות בין גרסאות
    public string? Id =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub")
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("nameid");

    public List<string>? Roles =>
        _httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList()
        is { Count: > 0 } roles ? roles
        : _httpContextAccessor.HttpContext?.User?
            .FindAll("role")
            .Select(x => x.Value)
            .ToList();
}
