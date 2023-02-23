using System.Collections.Immutable;

namespace Shell.Widget;

public record Widget(Guid WidgetId, string WidgetName, uint Count, bool IsArchived,  ImmutableDictionary<string, Reservation> Reservations);

public record Reservation(string Requester, uint RequestAmount);