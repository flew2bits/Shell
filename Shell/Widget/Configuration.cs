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
            .AddTransient<GetAll<AvailableWidget>>(svc => svc.GetRequiredService<WidgetData>().GetAvailableWidgets)
            .AddTransient<GetAll<TotalWidgetsSold>>(svc => svc.GetRequiredService<WidgetData>().GetSellCounts)
            .AddTransient<Find<Guid, WidgetReservationDetail>>(svc =>svc.GetRequiredService<WidgetData>().GetReservationDetail)
            .ConfigureMarten(config =>
            {
                config.Projections.Add<AvailableWidgetProjection>(ProjectionLifecycle.Inline);
                config.Projections.Add<TotalWidgetsSoldProjection>(ProjectionLifecycle.Inline);
                config.Projections.Add<WidgetReservationDetailProjection>(ProjectionLifecycle.Inline);

                config.Schema.For<AvailableWidget>().Identity(aw => aw.WidgetId);
                config.Schema.For<TotalWidgetsSold>().Identity(tws => tws.WidgetId);
                config.Schema.For<WidgetReservationDetail>().Identity(wrd => wrd.WidgetId);
            });

    public static IEndpointRouteBuilder MapWidgetApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/widget");
        group.MapGet("{widgetId:guid}", async (Guid widgetId, Find<Guid, WidgetReservationDetail> find) =>
            Results.Json(await find(widgetId)));

        return group;
    }
}