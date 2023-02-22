# Sample Entity

This folder contains everything needed to demonstrate a sample entity.

### Configure.cs
There is a single extension method for `IServiceCollection` that 
registers all of the necessary services for the entity. It also
configures the Marten projections.

From Program.cs, call `builder.Services.AddWidget();`