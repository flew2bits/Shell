using Marten.Events.Aggregation;
using Shell.Widget.Events;

namespace Shell.Widget.Views;

public record AvailableWidgets(Guid WidgetId, string Name, uint Inventory);

public class AvailableWidgetsProjection : SingleStreamAggregation<AvailableWidgets>
{
    public AvailableWidgetsProjection()
    {
        DeleteEvent<WidgetRemovedFromInventory>();
    }
    
    public AvailableWidgets Create(WidgetAdded added) => new(added.WidgetId, added.Name, added.InitialStock);

    public AvailableWidgets Apply(WidgetsSold sold, AvailableWidgets state) =>
        state with { Inventory = sold.InventoryRemaining };

    public AvailableWidgets Apply(WidgetStockReplenished replenished, AvailableWidgets state) =>
        state with { Inventory = replenished.CurrentInventory };
    
    
}