using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Constants;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace DeliveryManagementApp.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(Register, "register");
        group.MapPost(RegisterCourier, "register-courier").RequireAuthorization(policy => policy.RequireRole(Roles.Administrator));
        group.MapPost(RegisterAdmin, "register-admin").RequireAuthorization(policy => policy.RequireRole(Roles.Administrator));
        group.MapPost(Login, "login");
    }

    public static async Task<Results<Ok<string>, BadRequest<IEnumerable<string>>>> Register(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext dbContext,
        IJwtService jwtService,
        RegisterRequest request)
    {
        // Public registration is restricted to Customer role only
        if (request.Role != Roles.Customer)
            return TypedResults.BadRequest<IEnumerable<string>>(["Public registration is only allowed for the Customer role."]);

        return await CreateUser(userManager, roleManager, dbContext, jwtService, request);
    }

    public static async Task<Results<Ok<string>, BadRequest<IEnumerable<string>>>> RegisterCourier(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext dbContext,
        IJwtService jwtService,
        RegisterRequest request)
    {
        if (request.Role != Roles.Courier)
            return TypedResults.BadRequest<IEnumerable<string>>(["This endpoint only allows registering Courier accounts."]);

        return await CreateUser(userManager, roleManager, dbContext, jwtService, request);
    }

    public static async Task<Results<Ok<string>, BadRequest<IEnumerable<string>>>> RegisterAdmin(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext dbContext,
        IJwtService jwtService,
        RegisterRequest request)
    {
        if (request.Role != Roles.Administrator)
            return TypedResults.BadRequest<IEnumerable<string>>(["This endpoint only allows registering Administrator accounts."]);

        return await CreateUser(userManager, roleManager, dbContext, jwtService, request);
    }

    private static async Task<Results<Ok<string>, BadRequest<IEnumerable<string>>>> CreateUser(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext dbContext,
        IJwtService jwtService,
        RegisterRequest request)
    {
        if (!await roleManager.RoleExistsAsync(request.Role))
            return TypedResults.BadRequest<IEnumerable<string>>([$"Role '{request.Role}' does not exist."]);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Errors.Select(e => e.Description));

        var addRoleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!addRoleResult.Succeeded)
        {
            // Remove the created user to avoid leaving an account without the expected role
            await userManager.DeleteAsync(user);
            return TypedResults.BadRequest(addRoleResult.Errors.Select(e => e.Description));
        }

        // Read back the actual assigned role(s) and use that for further actions and token generation
        var roles = await userManager.GetRolesAsync(user);
        var assignedRole = roles.FirstOrDefault() ?? string.Empty;

        // Auto-create Customer or Courier profile based on the assigned role
        if (assignedRole == Roles.Customer)
        {
            dbContext.Add(new Customer
            {
                ApplicationUserId = user.Id,
                Phone = request.Phone ?? string.Empty
            });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        else if (assignedRole == Roles.Courier)
        {
            dbContext.Add(new Courier { ApplicationUserId = user.Id });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        var token = jwtService.GenerateToken(user.Id, user.Email!, assignedRole, user.FirstName, user.LastName);
        return TypedResults.Ok(token);
    }

    public static async Task<Results<Ok<string>, UnauthorizedHttpResult>> Login(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return TypedResults.Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded) return TypedResults.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var token = jwtService.GenerateToken(user.Id, user.Email!, role, user.FirstName, user.LastName);
        return TypedResults.Ok(token);
    }
}

public record RegisterRequest(string Email, string Password, string Role, string FirstName, string LastName, string? Phone);
public record LoginRequest(string Email, string Password);
