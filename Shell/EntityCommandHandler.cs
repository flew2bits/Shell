namespace Shell;

public abstract record EntityCommandHandler<TIdentity, TState>(
    Decider<TIdentity, TState> Decider,
    Loader<TIdentity, TState> LoadEntity,
    IEnumerable<Saver<TIdentity, TState>> EntitySavers,
    Archiver<TIdentity>? ArchiveIdentity = null
)
    where TState : class
{
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
}

public delegate Task<TState> Loader<in TIdentifier, TState>(TIdentifier id) where TState : class;

public delegate Task<bool> Saver<in TIdentifier, in TState>(TIdentifier id, TState state,
    IEnumerable<object> events)
    where TState : class;

public delegate void Archiver<in TIdentifier>(TIdentifier id);