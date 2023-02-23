namespace Shell.Widget.Events;

public record ReservationAdded(Guid WidgetId, string RequestedBy, string ReservationId, uint Amount);
