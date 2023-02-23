using Shell.Widget.Commands;
using Shell.Widget.Events;

namespace Shell.Widget;

public static class WidgetDecider
{
    // helper methods
    private static object[] Events(params object[] events) => events;
    private static object[] NoEvents => Array.Empty<object>();

    private static object[] Decide(Widget state, object command) =>
        command switch
        {
            AddWidget add => add.InitialStock > 0 && !state.IsArchived
                ? Events(new WidgetAdded(state.WidgetId, add.Name, add.InitialStock))
                : NoEvents,
            PurchaseNewStock amount => amount.WidgetCount > 0 && !state.IsArchived
                ? Events(new WidgetStockReplenished(state.WidgetId, amount.WidgetCount, state.Count + amount.WidgetCount))
                : NoEvents,
            SellWidgets sell => sell.WidgetCount <= state.Count && !state.IsArchived
                ? Events(new WidgetsSold(state.WidgetId, sell.WidgetCount, state.Count - sell.WidgetCount))
                : Events(new WidgetsNotSold(state.WidgetId, "Sell quantity exceeds inventory")),
            RemoveWidgetFromInventory => state is { Count: 0, IsArchived: false }
                ? Events(new WidgetRemovedFromInventory(state.WidgetId))
                : state.Count != 0 
                    ? Events(new WidgetNotRemovedFromInventory(state.WidgetId, "Cannot remove when inventory is on hand"))
                    : NoEvents,
            _ => NoEvents
        };

    private static Widget Evolve(Widget state, object @event) =>
        @event switch
        {
            WidgetAdded add => state with { Count = add.InitialStock, WidgetName = add.Name },
            WidgetsSold sold => state with { Count = sold.InventoryRemaining },
            WidgetStockReplenished replenished => state with { Count = replenished.CurrentInventory },
            WidgetsNotSold => state,
            WidgetRemovedFromInventory => state with { IsArchived = true },
            _ => state
        };

    private static Widget InitialState(Guid id) => new(id, "", 0, false);

    private static bool IsTerminal(Widget widget) => widget.IsArchived;

    private static bool IsCreator(object command) => command is AddWidget;

    public static readonly Decider<Guid, Widget> Decider = new(Decide, Evolve, InitialState, IsTerminal, IsCreator);
}