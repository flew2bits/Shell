namespace Shell.Widget.Events;

public record ReservationNotFulfilled(Guid WidgetId, string ReservationId, string Reason);