namespace Shell;

public abstract record EntityCommandHandler<TIdentity, TState> where TState : class
{
    protected EntityCommandHandler(Decider<TIdentity, TState> decider,
        Loader<TIdentity, TState> loadEntity,
        IEnumerable<Saver<TIdentity, TState>> entitySavers,
        Archiver<TIdentity>? archiveIdentity = null)
    {
        Decider = decider;
        LoadEntity = loadEntity;
        var savers = entitySavers.ToArray();
        EntitySavers = savers.Length > 0
            ? savers
            : throw new InvalidOperationException("At least one saver must be defined");
        ArchiveIdentity = archiveIdentity;
    }

    private async Task<TState?> TryLoad(TIdentity identity)
    {
        try
        {
            return await LoadEntity(identity);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public async Task<(TState, IEnumerable<object>)> HandleCommand(TIdentity identity, object command)
    {
        var maybeState = await TryLoad(identity);

        var state = (maybeState is not null, Decider.IsCreator(command)) switch
        {
            (false, true) => Decider.InitialState(identity),
            (true, false) => maybeState!,
            (true, true) => throw new InvalidOperationException("An entity with the given id already exists"),
            (false, false) => throw new InvalidOperationException("Could not find the entity with the given id")
        };

        var (newState, events) = Decider.Handle(state, command);

        // don't try to save if nothing happened
        if (!events.Any()) return (state, Array.Empty<object>());

        foreach (var saver in EntitySavers)
        {
            if (!await saver(identity, newState, events)) break;
        }

        if (Decider.IsFinal(newState))
        {
            ArchiveIdentity?.Invoke(identity);
        }

        return (newState, events);
    }

    public Decider<TIdentity, TState> Decider { get; }
    public Loader<TIdentity, TState> LoadEntity { get; }
    public IEnumerable<Saver<TIdentity, TState>> EntitySavers { get; }
    public Archiver<TIdentity>? ArchiveIdentity { get; }

    public void Deconstruct(out Decider<TIdentity, TState> decider, out Loader<TIdentity, TState> loadEntity,
        out IEnumerable<Saver<TIdentity, TState>> entitySavers, out Archiver<TIdentity>? archiveIdentity)
    {
        decider = Decider;
        loadEntity = LoadEntity;
        entitySavers = EntitySavers;
        archiveIdentity = ArchiveIdentity;
    }
}

public delegate Task<TState> Loader<in TIdentifier, TState>(TIdentifier id) where TState : class;

public delegate Task<bool> Saver<in TIdentifier, in TState>(TIdentifier id, TState state,
    IEnumerable<object> events)
    where TState : class;

public delegate void Archiver<in TIdentifier>(TIdentifier id);