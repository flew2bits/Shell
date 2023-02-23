namespace Shell.Widget.Events;

public record ReservationFulfilled(Guid WidgetId, string ReservationId, uint CurrentInventory, uint Quantity);