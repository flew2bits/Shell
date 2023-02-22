namespace Shell;

public delegate Task<IEnumerable<TView>> GetAll<TView>();

public delegate Task<TView?> Find<in TIdentity, TView>(TIdentity id);