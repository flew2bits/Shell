using Marten.Events.Aggregation;
using Shell.Widget.Events;

namespace Shell.Widget.Views;

public record WidgetReservationDetail(Guid WidgetId, string Name, Reservation[] Reservations);

public record Reservation(string ReservationId, string ReservedFor, uint Requested);

public class WidgetReservationDetailProjection : SingleStreamAggregation<WidgetReservationDetail>
{
    public WidgetReservationDetailProjection()
    {
        DeleteEvent<WidgetRemovedFromInventory>();
    }
    
    public WidgetReservationDetail Create(WidgetAdded added) =>
        new WidgetReservationDetail(added.WidgetId, added.Name, Array.Empty<Reservation>());

    public WidgetReservationDetail Apply(ReservationAdded reserved, WidgetReservationDetail state) =>
        state with
        {
            Reservations = state.Reservations
                .Append(new Reservation(reserved.ReservationId, reserved.RequestedBy, reserved.Amount)).ToArray()
        };

    public WidgetReservationDetail Apply(ReservationFulfilled fulfilled, WidgetReservationDetail state) =>
        state with
        {
            Reservations = state.Reservations.Where(r => r.ReservationId != fulfilled.ReservationId).ToArray()
        };
}