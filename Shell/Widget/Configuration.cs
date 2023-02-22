using Marten;
using Marten.Events.Projections;
using Shell.Widget.Views;

namespace Shell.Widget;

public static class Configuration
{
    public static IServiceCollection AddWidget(this IServiceCollection services) =>
        services
            .AddSingleton(WidgetDecider.Decider)
            .AddSingleton<Evolver<Guid, Widget>>(WidgetDecider.Decider)
            .AddScoped<WidgetCommandHandler>()
            .AddScoped<WidgetData>()
            .AddScoped<Loader<Guid, Widget>>(svc => svc.GetRequiredService<WidgetData>().Load)
            .AddScoped<Saver<Guid, Widget>>(svc => svc.GetRequiredService<WidgetData>().Save)

        // Everything below is related to projections
            
            .AddTransient<GetAll<AvailableWidgets>>(svc => svc.GetRequiredService<WidgetData>().GetAvailableWidgets)
            .AddTransient<GetAll<TotalWidgetsSold>>(svc => svc.GetRequiredService<WidgetData>().GetSellCounts)
            .ConfigureMarten(config =>
            {
                config.Projections.Add<AvailableWidgetsProjection>(ProjectionLifecycle.Inline);
                config.Projections.Add<TotalWidgetsSoldProjection>(ProjectionLifecycle.Inline);

                config.Schema.For<AvailableWidgets>().Identity(aw => aw.WidgetId);
                config.Schema.For<TotalWidgetsSold>().Identity(tws => tws.WidgetId);
            })
        ;
}