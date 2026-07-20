namespace DeliveryManagementApp.Domain.Entities;

public class Route : BaseAuditableEntity
{
    public int CourierId { get; set; }
    public int? VehicleId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    /// <summary>Persisted courier position (1-based). Null = route not started.</summary>
    public int? CurrentStop { get; set; }

    // Navigation
    public Courier Courier { get; set; } = null!;
    public Vehicle? Vehicle { get; set; }
    public ICollection<RouteItem> Items { get; set; } = new List<RouteItem>();

    // Add a stop to the route. Position is 1-based. If position is null, append to the end.
    public RouteItem AddStop(Order order, StopType stopType = StopType.Delivery, int? position = null)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));

        // Determine insert position (1-based)
        var max = Items.Count == 0 ? 0 : Items.Max(i => i.StopOrder);
        var insertPos = position ?? (max + 1);

        if (insertPos < 1 || insertPos > (max + 1))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 1 and Items.Count+1");
        }

        // Shift existing items at or after insertPos
        foreach (var it in Items.Where(i => i.StopOrder >= insertPos))
        {
            it.StopOrder++;
        }

        var routeItem = new RouteItem
        {
            OrderId = order.Id,
            Order = order,
            Route = this,
            RouteId = this.Id,
            StopOrder = insertPos,
            StopType = stopType
        };

        Items.Add(routeItem);
        return routeItem;
    }

    // Remove the stop at a given 1-based position. Returns true if removed.
    public bool RemoveStopAt(int position)
    {
        if (position < 1) return false;

        var item = Items.SingleOrDefault(i => i.StopOrder == position);
        if (item is null) return false;

        Items.Remove(item);

        // Shift down stops after removed position
        foreach (var it in Items.Where(i => i.StopOrder > position))
        {
            it.StopOrder--;
        }

        return true;
    }

    // Move a stop from one 1-based position to another
    public void MoveStop(int fromPosition, int toPosition)
    {
        if (fromPosition == toPosition) return;

        var count = Items.Count;
        if (fromPosition < 1 || fromPosition > count) throw new ArgumentOutOfRangeException(nameof(fromPosition));
        if (toPosition < 1 || toPosition > count) throw new ArgumentOutOfRangeException(nameof(toPosition));

        var item = Items.Single(i => i.StopOrder == fromPosition);

        if (fromPosition < toPosition)
        {
            // shift items between fromPosition+1 .. toPosition down by 1
            foreach (var it in Items.Where(i => i.StopOrder > fromPosition && i.StopOrder <= toPosition))
            {
                it.StopOrder--;
            }
        }
        else
        {
            // shift items between toPosition .. fromPosition-1 up by 1
            foreach (var it in Items.Where(i => i.StopOrder >= toPosition && i.StopOrder < fromPosition))
            {
                it.StopOrder++;
            }
        }

        item.StopOrder = toPosition;
    }
}
