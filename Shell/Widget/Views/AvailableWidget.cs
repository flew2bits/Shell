using Marten.Events.Aggregation;
using Shell.Widget.Events;

namespace Shell.Widget.Views;

public record AvailableWidget(Guid WidgetId, string Name, uint Inventory);

public class AvailableWidgetProjection : SingleStreamAggregation<AvailableWidget>
{
    public AvailableWidgetProjection()
    {
        DeleteEvent<WidgetRemovedFromInventory>();
    }
    
    public AvailableWidget Create(WidgetAdded added) => new(added.WidgetId, added.Name, added.InitialStock);

    public AvailableWidget Apply(WidgetsSold sold, AvailableWidget state) =>
        state with { Inventory = sold.InventoryRemaining };

    public AvailableWidget Apply(WidgetStockReplenished replenished, AvailableWidget state) =>
        state with { Inventory = replenished.CurrentInventory };
    
    
}