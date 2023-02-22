namespace Shell.Widget;

public record WidgetCommandHandler(Decider<Guid, Widget> Decider, Loader<Guid, Widget> LoadEntity,
    IEnumerable<Saver<Guid, Widget>> EntitySavers) :
    EntityCommandHandler<Guid, Widget>(Decider, LoadEntity, EntitySavers);