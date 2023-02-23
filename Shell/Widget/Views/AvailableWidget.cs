using Marten.Events.Aggregation;
using Shell.Widget.Events;

namespace Shell.Widget.Views;

public record AvailableWidget(Guid WidgetId, string Name, uint Inventory, uint Reserved);

public class AvailableWidgetProjection : SingleStreamAggregation<AvailableWidget>
{
    public AvailableWidgetProjection()
    {
        DeleteEvent<WidgetRemovedFromInventory>();
    }
    
    public AvailableWidget Create(WidgetAdded added) => new(added.WidgetId, added.Name, added.InitialStock, 0);

    public AvailableWidget Apply(WidgetsSold sold, AvailableWidget state) =>
        state with { Inventory = sold.InventoryRemaining };

    public AvailableWidget Apply(WidgetStockReplenished replenished, AvailableWidget state) =>
        state with { Inventory = replenished.CurrentInventory };

    public AvailableWidget Apply(ReservationAdded reserved, AvailableWidget state) =>
        state with { Reserved = state.Reserved + reserved.Amount };

    public AvailableWidget Apply(ReservationFulfilled fulfilled, AvailableWidget state) =>
        state with { Reserved = state.Reserved - fulfilled.Quantity, Inventory = fulfilled.CurrentInventory };
}