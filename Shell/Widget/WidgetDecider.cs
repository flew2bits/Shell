using System.Collections.Immutable;
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
                ? Events(new WidgetStockReplenished(state.WidgetId, amount.WidgetCount,
                    state.Count + amount.WidgetCount))
                : NoEvents,

            SellWidgets sell => state.IsArchived ? NoEvents :
                    sell.WidgetCount <= state.Count - state.Reservations.Values.Sum(r => r.RequestAmount)
                ? Events(new WidgetsSold(state.WidgetId, sell.WidgetCount, state.Count - sell.WidgetCount))
                : Events(new WidgetsNotSold(state.WidgetId, "Sell quantity exceeds unreserved inventory")),

            RemoveWidgetFromInventory => HandleRemoveWidgetFromInventory(state),

            ReserveWidgets reserve => state.IsArchived
                ? NoEvents
                : state.Reservations.ContainsKey(reserve.RequestId)
                    ? Events(
                        new ReservationNotAdded(state.WidgetId, reserve.RequestId, "Reservation ID already exists"))
                    : Events(new ReservationAdded(state.WidgetId, reserve.RequestedBy, reserve.RequestId,
                        reserve.RequestAmount)),

            FulfillReservation fulfill => HandleFulfillReservation(fulfill, state),
            _ => NoEvents
        };

    private static object[] HandleFulfillReservation(FulfillReservation cmd, Widget state)
    {
        if (state.Reservations.TryGetValue(cmd.RequestId, out var reservation) && !state.IsArchived &&
            reservation.RequestAmount <= state.Count)
            return Events(new ReservationFulfilled(state.WidgetId, cmd.RequestId,
                state.Count - reservation.RequestAmount, reservation.RequestAmount));

        if (state.IsArchived) return NoEvents;

        if (reservation is null)
            return Events(new ReservationNotFulfilled(state.WidgetId, cmd.RequestId,
                $@"Reservation ""{cmd.RequestId}"" does not exist for this widget"));

        return Events(new ReservationNotFulfilled(state.WidgetId, cmd.RequestId,
            "Reservation amount exceeds quantity on hand"));
    }

    private static object[] HandleRemoveWidgetFromInventory(Widget state)
    {
        if (state is { Count: 0, Reservations.Count: 0, IsArchived: false })
            return Events(new WidgetRemovedFromInventory(state.WidgetId));

        if (state.IsArchived) return NoEvents;

        var reasons = new List<string>();

        if (state.Count != 0) reasons.Add("Cannot remove while inventory remains");
        if (state.Reservations.Any()) reasons.Add("Cannot remove while reservations are unfulfilled");

        return Events(new WidgetNotRemovedFromInventory(state.WidgetId, string.Join(", ", reasons)));
    }

    private static Widget Evolve(Widget state, object @event) =>
        @event switch
        {
            WidgetAdded add => state with { Count = add.InitialStock, WidgetName = add.Name },
            WidgetsSold sold => state with { Count = sold.InventoryRemaining },
            WidgetStockReplenished replenished => state with { Count = replenished.CurrentInventory },
            WidgetsNotSold => state,
            WidgetRemovedFromInventory => state with { IsArchived = true },

            ReservationAdded res => state with
            {
                Reservations = state.Reservations.Add(res.ReservationId, new Reservation(res.RequestedBy, res.Amount))
            },
            
            ReservationFulfilled fulfilled => state with
            {
                Reservations = state.Reservations.Remove(fulfilled.ReservationId),
                Count = fulfilled.CurrentInventory
            },
            _ => state
        };

    private static Widget InitialState(Guid id) => new(id, "", 0, false, ImmutableDictionary<string, Reservation>.Empty);

    private static bool IsTerminal(Widget widget) => widget.IsArchived;

    private static bool IsCreator(object command) => command is AddWidget;

    public static readonly Decider<Guid, Widget> Decider = new(Decide, Evolve, InitialState, IsTerminal, IsCreator);
}