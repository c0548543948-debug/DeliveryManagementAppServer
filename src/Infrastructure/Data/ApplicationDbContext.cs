using System.Reflection;
using DeliveryManagementApp.Application.Common.Interfaces;
using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.Common;
using DeliveryManagementApp.Infrastructure.Identity;
using DeliveryManagementApp.Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DeliveryManagementApp.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    // Keep DbSet properties for EF usage internally
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<RouteItem> RouteItems => Set<RouteItem>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Expose IQueryable<T> for the Application layer via the interface
    IQueryable<Order> IApplicationDbContext.Orders => Orders;
    IQueryable<Customer> IApplicationDbContext.Customers => Customers;
    IQueryable<Courier> IApplicationDbContext.Couriers => Couriers;
    IQueryable<Vehicle> IApplicationDbContext.Vehicles => Vehicles;
    IQueryable<Route> IApplicationDbContext.Routes => Routes;
    IQueryable<RouteItem> IApplicationDbContext.RouteItems => RouteItems;
    IQueryable<ActivityLog> IApplicationDbContext.ActivityLogs => ActivityLogs;

    // Executor implementation
    async Task<List<T>> IApplicationDbContext.ExecuteQueryAsync<T>(IQueryable<T> query, CancellationToken cancellationToken)
        where T : class
        => await query.ToListAsync(cancellationToken);

    async Task<Route?> IApplicationDbContext.GetRouteWithItemsAsync(int routeId, CancellationToken cancellationToken)
        => await Routes
            .Include(r => r.Items)
            .ThenInclude(i => i.Order)
            .FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);
    void IApplicationDbContext.Add<T>(T entity) where T : class => Set<T>().Add(entity);
    void IApplicationDbContext.Remove<T>(T entity) where T : class => Set<T>().Remove(entity);

    async Task<T?> IApplicationDbContext.FindAsync<T>(object id, CancellationToken cancellationToken) where T : class
        => await Set<T>().FindAsync(new[] { id }, cancellationToken) as T;

    async Task<bool> IApplicationDbContext.AnyAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        => await Set<T>().AnyAsync(predicate, cancellationToken);

    async Task<T?> IApplicationDbContext.ExecuteSingleAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) where T : class
        => await query.FirstOrDefaultAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // Populate audit fields before saving changes
    private void UpdateAuditableEntities()
    {
        var user = this.GetService<IUser>();
        var dateTime = this.GetService<TimeProvider>();
        var utcNow = DateOnly.FromDateTime((dateTime?.GetUtcNow() ?? DateTimeOffset.UtcNow).UtcDateTime);

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.Created = utcNow;
                entry.Entity.CreatedBy = user?.Id;
                entry.Entity.LastModified = utcNow;
                entry.Entity.LastModifiedBy = user?.Id;
            }
            else if (entry.State is EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.LastModified = utcNow;
                entry.Entity.LastModifiedBy = user?.Id;
            }
        }
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
