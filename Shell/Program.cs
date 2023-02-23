using FluentValidation;
using Marten;
using Marten.Services.Json;
using Shell.Widget;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(config =>
{
    config.Connection(builder.Configuration.GetConnectionString("Marten") ??
                      throw new InvalidOperationException("Could not find connection string"));
    config.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
    
    // this should probably not be used in production
    config.AutoCreateSchemaObjects = AutoCreate.All;
});
builder.Services.AddRazorPages();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddWidget();

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();

app.Run();