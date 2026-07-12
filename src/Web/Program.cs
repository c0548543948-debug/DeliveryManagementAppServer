using DeliveryManagementApp.Infrastructure.Data;
using DeliveryManagementApp.Web.Hubs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UseCors(builder =>
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
            (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
        .AllowCredentials());

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);
app.MapHub<TrackingHub>("/hubs/tracking");

app.MapFallbackToFile("index.html");

app.Run();
