namespace Shell.Widget.Events;

public record WidgetStockReplenished(Guid WidgetId, uint Purchased, uint CurrentInventory);
