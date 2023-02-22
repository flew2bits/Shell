namespace Shell;

public record Evolver<TIdentity, TState>(
    Func<TState, object, TState> Evolve,
    Func<TIdentity, TState> InitialState
) where TState : class;

public record Decider<TIdentity, TState>(
        Func<TState, object, IEnumerable<object>> Decide,
        Func<TState, object, TState> Evolve,
        Func<TIdentity, TState> InitialState,
        Predicate<TState> IsFinal,
        Predicate<object> IsCreator)
    : Evolver<TIdentity, TState>(Evolve, InitialState)
    where TState : class
{
    public (TState, object[]) Handle(TState state, object command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        var events = Decide(state, command).ToArray();
        var newState = events.Aggregate(state, Evolve);
        return (newState, events);
    }
}