using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shell.Widget;
using Shell.Widget.Commands;
using Shell.Widget.Events;
using Shell.Widget.Views;

namespace Shell.Pages.Widget;

public class Index : PageModel
{
    private readonly WidgetCommandHandler _commandHandler;

    public Index(WidgetCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    public AvailableWidget[] Widgets { get; set; } = Array.Empty<AvailableWidget>();

    public TotalWidgetsSold[] Sales { get; set; } = Array.Empty<TotalWidgetsSold>();

    [BindProperty] public AddWidgetRequest AddWidget { get; set; } = new("", 0);
    [BindProperty] public SellWidgetRequest SellWidgets { get; set; } = new(Guid.Empty, 0);
    [BindProperty] public BuyWidgetsRequest BuyWidgets { get; set; } = new(Guid.Empty, 0);
    [BindProperty] public ReserveWidgetsRequest ReserveWidgets { get; set; } = new(Guid.Empty, 0, "", "");

    [BindProperty] public FulfillReservationRequest FulfillReservation { get; set; } = new(Guid.Empty);

    public async Task OnGet([FromServices] GetAll<AvailableWidget> getAvailableWidgets,
        [FromServices] GetAll<TotalWidgetsSold> getSales)
    {
        Widgets = (await getAvailableWidgets()).ToArray();
        Sales = (await getSales()).ToArray();
    }

    public async Task<IActionResult> OnPostAddWidget()
    {
        var x = FunctionalCore.Widget.Command.NewAddWidget(AddWidget.Name, AddWidget.InitialInventory);
        await _commandHandler.HandleCommand(Guid.NewGuid(), new AddWidget(AddWidget.Name, AddWidget.InitialInventory));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSellWidgets()
    {
        var (_, events) = await _commandHandler.HandleCommand(SellWidgets.WidgetId, new SellWidgets(SellWidgets.Sell));
        if (events.SingleOrDefault() is WidgetsNotSold w)
            TempData["Message"] = $"Could not sell widgets: {w.Reason}";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBuyWidgets()
    {
        await _commandHandler.HandleCommand(BuyWidgets.WidgetId, new PurchaseNewStock(BuyWidgets.Buy));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReserveWidgets()
    {
        if (string.IsNullOrEmpty(ReserveWidgets.ReservationId) || string.IsNullOrEmpty(ReserveWidgets.ReservedFor))
            return RedirectToPage();

        var(_, events) = await _commandHandler.HandleCommand(ReserveWidgets.WidgetId,
            new ReserveWidgets(ReserveWidgets.ReservedFor, ReserveWidgets.ReservationId, ReserveWidgets.Reserve));

        if (events.SingleOrDefault() is ReservationNotAdded fail)
        {
            TempData["Message"] = $"Reservation failed: {fail.Reason}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostFulfillReservation(string reservationId)
    {
        var (_, events) = await _commandHandler.HandleCommand(FulfillReservation.WidgetId,
            new FulfillReservation(reservationId));

        if (events.SingleOrDefault() is ReservationNotFulfilled fail)
            TempData["Message"] = $"Could not fulfill: {fail.Reason}";

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostRemoveWidgetFromInventory(Guid widgetId)
    {
        var (_, events) = await _commandHandler.HandleCommand(widgetId, new RemoveWidgetFromInventory());
        if (events.SingleOrDefault() is WidgetNotRemovedFromInventory w)
            TempData["Message"] = $"Could not remove from inventory: {w.Reason}";
        return RedirectToPage();
    }

    public record AddWidgetRequest(string Name, uint InitialInventory);

    public record SellWidgetRequest(Guid WidgetId, uint Sell);

    public record BuyWidgetsRequest(Guid WidgetId, uint Buy);

    public record ReserveWidgetsRequest(Guid WidgetId, uint Reserve, string ReservedFor, string ReservationId);

    public record FulfillReservationRequest(Guid WidgetId);
}