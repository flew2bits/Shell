using Marten.Events.Aggregation;
using Shell.Widget.Events;

namespace Shell.Widget.Views;

public record TotalWidgetsSold(Guid WidgetId, string Name, uint TotalCount);

public class TotalWidgetsSoldProjection : SingleStreamAggregation<TotalWidgetsSold>
{
    public TotalWidgetsSold Create(WidgetAdded add) => new(add.WidgetId, add.Name, 0);

    public TotalWidgetsSold Apply(WidgetsSold sold, TotalWidgetsSold state) =>
        state with { TotalCount = state.TotalCount + sold.Sold };
}