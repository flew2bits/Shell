using Marten;
using Marten.Linq;
using Shell.Infrastructure;
using Shell.Widget.Views;

namespace Shell.Widget;

public class WidgetData : MartenData<Widget>
{
    public WidgetData(IDocumentStore store, Evolver<Guid, Widget> evolver) : base(store, evolver)
    {
    }
    
    public async Task<IEnumerable<AvailableWidget>> GetAvailableWidgets()
    {
        await using var session = Store.QuerySession();
        return await session.Query<AvailableWidget>().ToListAsync();
    }
    
    public async Task<IEnumerable<TotalWidgetsSold>> GetSellCounts()
    {
        await using var session = Store.QuerySession();
        return await session.Query<TotalWidgetsSold>().ToListAsync();
    }

    public async Task<WidgetReservationDetail?> GetReservationDetail(Guid widgetId)
    {
        await using var session = Store.QuerySession();
        return await session.Query<WidgetReservationDetail>().SingleOrDefaultAsync(w => w.WidgetId == widgetId);
    }
}