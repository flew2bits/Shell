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
    
    [BindProperty] public AddWidgetRequest AddWidget { get; set; } = new("", 0);
    [BindProperty] public SellWidgetRequest SellWidgets { get; set; } = new(Guid.Empty, 0);
    [BindProperty] public BuyWidgetsRequest BuyWidgets { get; set; } = new(Guid.Empty, 0);

    public async Task OnGet([FromServices] GetAll<AvailableWidget> getAvailableWidgets)
    {
        Widgets = (await getAvailableWidgets()).ToArray();
    }

    public async Task<IActionResult> OnPostAddWidget()
    {
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
}