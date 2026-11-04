using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionScheduleLocator
{
    private static readonly Regex StartInName = new(
        @"Begin From (?<day>[A-Za-z]+, [A-Za-z]+ \d{1,2}, \d{4}) at (?<time>\d{1,2}:\d{2}:\d{2})\W?(?<half>AM|PM)");

    private readonly IPage _page;
    private readonly string _componentId;

    public FusionScheduleLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Appointments(string text) => Root.Locator(".e-appointment").Filter(new() { HasText = text });

    public ILocator Appointments(Regex text) => Root.Locator(".e-appointment").Filter(new() { HasTextRegex = text });

    public ILocator Appointment(string subject, DateTime start, int groupIndex) =>
        Root.Locator($".e-appointment[data-group-index='{groupIndex}']").And(
            Root.GetByRole(AriaRole.Button, new()
            {
                NameRegex = new Regex(
                    Regex.Escape($"{subject} Begin From {start.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture)} at " +
                                 start.ToString("h:mm:ss", CultureInfo.InvariantCulture)) +
                    @"\W?" + start.ToString("tt", CultureInfo.InvariantCulture)),
            }));

    public async Task<DateTime> StartOf(ILocator appointment)
    {
        var name = await appointment.GetAttributeAsync("aria-label") ?? "";
        var start = StartInName.Match(name);
        if (!start.Success) throw new InvalidOperationException($"The appointment's name carries no start: '{name}'.");

        return DateTime.ParseExact(
            $"{start.Groups["day"].Value} {start.Groups["time"].Value} {start.Groups["half"].Value}",
            "dddd, MMMM d, yyyy h:mm:ss tt",
            CultureInfo.InvariantCulture);
    }

    public async Task<int> GroupOf(ILocator appointment) =>
        int.Parse(await appointment.GetAttributeAsync("data-group-index") ?? "", CultureInfo.InvariantCulture);

    public ILocator Cell(DateTime start, int groupIndex) =>
        Root.Locator($"td.e-work-cells[data-group-index='{groupIndex}'][data-date='{new DateTimeOffset(start).ToUnixTimeMilliseconds()}']");

    public async Task ClickEmptyPartOf(ILocator cell)
    {
        await cell.ScrollIntoViewIfNeededAsync();
        var box = await cell.BoundingBoxAsync() ?? throw new InvalidOperationException("The cell is not on screen.");
        await cell.ClickAsync(new() { Position = new() { X = 4, Y = box.Height - 4 } });
    }

    public ILocator ViewButton(string view) =>
        Root.Locator(".e-schedule-toolbar").GetByRole(AriaRole.Button, new() { Name = view, Exact = true });

    public ILocator Next => Root.Locator(".e-toolbar-item.e-next button");

    public ILocator DateRange => Root.Locator(".e-toolbar-item.e-date-range");

    public ILocator Details => _page.Locator(".e-quick-popup-wrapper.e-popup-open");

    public ILocator DetailsClose => _page.Locator(".e-quick-popup-wrapper .e-close");

    public ILocator Editor => _page.Locator($"#{_componentId}_dialog_wrapper");

    public ILocator EditorField(string field) => Editor.Locator($"[name='{field}']");

    public ILocator EditorCancel => Editor.Locator(".e-event-cancel");

    public ILocator EditorSave => Editor.Locator(".e-event-save");

    public ILocator EditorDelete => Editor.Locator(".e-event-delete");

    public ILocator Alert => _page.Locator($"#{_componentId}QuickDialog");

    public ILocator ConfirmDelete => Alert.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true });

    public async Task Open(ILocator appointment)
    {
        await appointment.ScrollIntoViewIfNeededAsync();
        await appointment.ClickAsync();
    }

    public async Task OpenToChange(ILocator appointment)
    {
        await appointment.ScrollIntoViewIfNeededAsync();
        await appointment.DblClickAsync();
    }
}
