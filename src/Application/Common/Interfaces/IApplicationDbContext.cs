using DeliveryManagementApp.Domain.Entities;
using System.Linq.Expressions;

namespace DeliveryManagementApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Expose IQueryable<T> for each aggregate root so handlers can build LINQ queries
    // and use ProjectTo without depending on EF Core types directly.
    IQueryable<Order> Orders { get; }
    IQueryable<Customer> Customers { get; }
    IQueryable<Courier> Couriers { get; }
    IQueryable<Vehicle> Vehicles { get; }
    IQueryable<Route> Routes { get; }
    IQueryable<RouteItem> RouteItems { get; }
    IQueryable<ActivityLog> ActivityLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // Pragmatic executor: materialize an IQueryable<T> to a List<T> asynchronously.
    // Implementations in Infrastructure will delegate to EF Core's ToListAsync.
    Task<List<T>> ExecuteQueryAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;

    // Domain-specific convenience for route retrieval with related items and orders.
    Task<Route?> GetRouteWithItemsAsync(int routeId, CancellationToken cancellationToken = default);

    // Basic change-tracking helpers
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;

    // Find by primary key (simple overload for common PK patterns)
    Task<T?> FindAsync<T>(object id, CancellationToken cancellationToken = default) where T : class;

    // Predicate-based existence check
    Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;

    // Materialize a single item (FirstOrDefaultAsync/SingleOrDefaultAsync equivalent)
    Task<T?> ExecuteSingleAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class;
}
