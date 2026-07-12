using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryManagementApp.Infrastructure.Services;

public class TrackingTokenService : ITrackingTokenService
{
    private readonly JwtSettings _jwtSettings;

    public TrackingTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(int orderId, int courierId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var claims = new[]
        {
            new Claim("orderId", orderId.ToString()),
            new Claim("courierId", courierId.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (int OrderId, int CourierId)? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key
            }, out _);

            var orderId = int.Parse(principal.FindFirstValue("orderId")!);
            var courierId = int.Parse(principal.FindFirstValue("courierId")!);
            return (orderId, courierId);
        }
        catch
        {
            return null;
        }
    }
}
