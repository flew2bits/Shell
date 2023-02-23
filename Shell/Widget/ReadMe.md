# Sample Entity: Widget

This folder contains everything needed to demonstrate a sample entity.

### Commands/ and Events/
These directories contain all of the events and commands for
the entity. They are all immutable record types, and all very
simple. Adding new behavior requires creating a new command and
corresponding event(s). Once these are created, the `Decider.Decide`
method should be updated to check the state and command to
generate the new events; the `Decider.Evolve` method should be
updated to take the events and transition the state.



### Configure.cs
There is a single extension method for `IServiceCollection` that 
registers all of the necessary services for the entity. It also
configures the Marten projections.

From Program.cs, call `builder.Services.AddWidget();`

### Widget.cs
This contains the state object for the entity. It is only used
during command handling. In this sample, the Widget is an
immutable record class. It's possible to use a mutable class (or record)
for memory efficiency. You can also choose to save the current
state instead of the event stream. Everything will work just
the same, but you'll have to make use of a different Saver and
Loader.

### WidgetCommandHandler.cs
This is the "Imperative Shell" part of the widget. It extends
the EntityCommandHandler abstract class, simply passing in 
the Decider, Loader, and Saver, both from WidgetData.cs. 

### WidgetData.cs
This is the data access class for the Widget entity. It extends
MartenData, which provides the event streamed Load and Save
methods (used by WidgetCommandHandler). The additional methods
defined in this class are related to projections/views.