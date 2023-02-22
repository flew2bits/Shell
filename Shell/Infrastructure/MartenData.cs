using Marten;

namespace Shell.Infrastructure;

public abstract class MartenData<TEntity> where TEntity:class
{
    protected readonly IDocumentStore Store;
    private readonly Evolver<Guid, TEntity> _evolver;

    protected MartenData(IDocumentStore store, Evolver<Guid, TEntity> evolver)
    {
        Store = store;
        _evolver = evolver;
    }

    public async Task<TEntity> Load(Guid id)
    {
        await using var session = Store.QuerySession();
        var events = await session.Events.FetchStreamAsync(id);
        if (!events.Any()) throw new InvalidOperationException("Entity does not exist");
        return events.Select(e => e.Data).Aggregate(_evolver.InitialState(id), _evolver.Evolve);
    }

    public async Task<bool> Save(Guid id, TEntity _, IEnumerable<object> events)
    {
        await using var session = Store.LightweightSession();
        session.Events.Append(id, events);
        await session.SaveChangesAsync();
        return true;
    }
}