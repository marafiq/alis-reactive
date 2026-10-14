using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// A Syncfusion Schedule: its appointments, its cells, its header bar, the details popup an appointment opens
/// and its own event editor.
/// </summary>
public sealed class FusionScheduleLocator
{
    // The name the schedule gives an appointment for screen readers carries its start: "... Begin From Tuesday,
    // October 13, 2026 at 4:00:00 PM ...". Newer browsers put a narrow space before AM or PM.
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

    /// <summary>The appointments that show this text.</summary>
    public ILocator Appointments(string text) => Root.Locator(".e-appointment").Filter(new() { HasText = text });

    /// <summary>The appointments whose text matches.</summary>
    public ILocator Appointments(Regex text) => Root.Locator(".e-appointment").Filter(new() { HasTextRegex = text });

    /// <summary>
    /// The appointment with this subject that starts at this time, in a resource group, by the name the schedule
    /// gives it for screen readers. An appointment that runs past midnight shows in both days' columns.
    /// </summary>
    public ILocator Appointment(string subject, DateTime start, int groupIndex) =>
        Root.Locator($".e-appointment[data-group-index='{groupIndex}']").And(
            Root.GetByRole(AriaRole.Button, new()
            {
                NameRegex = new Regex(
                    Regex.Escape($"{subject} Begin From {start.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture)} at " +
                                 start.ToString("h:mm:ss", CultureInfo.InvariantCulture)) +
                    @"\W?" + start.ToString("tt", CultureInfo.InvariantCulture)),
            }));

    /// <summary>When an appointment starts, as the name the schedule gives it for screen readers says.</summary>
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

    /// <summary>The resource group an appointment shows in, numbered from 0.</summary>
    public async Task<int> GroupOf(ILocator appointment) =>
        int.Parse(await appointment.GetAttributeAsync("data-group-index") ?? "", CultureInfo.InvariantCulture);

    /// <summary>
    /// A cell of the schedule's grid by its start and its resource group: an hour of a day or week view, or a day
    /// of the month view (its start is that day's midnight). The cell's <c>data-date</c> is its start in the
    /// browser's zone.
    /// </summary>
    public ILocator Cell(DateTime start, int groupIndex) =>
        Root.Locator($"td.e-work-cells[data-group-index='{groupIndex}'][data-date='{new DateTimeOffset(start).ToUnixTimeMilliseconds()}']");

    /// <summary>
    /// Clicks a cell where it is empty: its lower-left corner, below the appointments a month's day shows and
    /// beside the indicator of more.
    /// </summary>
    public async Task ClickEmptyPartOf(ILocator cell)
    {
        await cell.ScrollIntoViewIfNeededAsync();
        var box = await cell.BoundingBoxAsync() ?? throw new InvalidOperationException("The cell is not on screen.");
        await cell.ClickAsync(new() { Position = new() { X = 4, Y = box.Height - 4 } });
    }

    /// <summary>The header bar's button for a view, by the view's name: "Day", "Week", "Month".</summary>
    public ILocator ViewButton(string view) =>
        Root.Locator(".e-schedule-toolbar").GetByRole(AriaRole.Button, new() { Name = view, Exact = true });

    /// <summary>The header bar's button that moves to the next week, month or day.</summary>
    public ILocator Next => Root.Locator(".e-toolbar-item.e-next button");

    /// <summary>The dates the header bar says the schedule shows.</summary>
    public ILocator DateRange => Root.Locator(".e-toolbar-item.e-date-range");

    /// <summary>The details popup an appointment opens, while it is open.</summary>
    public ILocator Details => _page.Locator(".e-quick-popup-wrapper.e-popup-open");

    /// <summary>The close button of the details popup.</summary>
    public ILocator DetailsClose => _page.Locator(".e-quick-popup-wrapper .e-close");

    /// <summary>The schedule's own event editor.</summary>
    public ILocator Editor => _page.Locator($"#{_componentId}_dialog_wrapper");

    /// <summary>A field of the event editor, by the event field it edits: "subject", "description".</summary>
    public ILocator EditorField(string field) => Editor.Locator($"[name='{field}']");

    /// <summary>The event editor's Cancel button.</summary>
    public ILocator EditorCancel => Editor.Locator(".e-event-cancel");

    /// <summary>
    /// Opens an appointment's details the way a hand does: bring it into view, then click it. A bare click on an
    /// appointment outside the visible hours scrolls and clicks in the same frame, the scroll lands after the
    /// click, and the schedule closes its details on any scroll of its content.
    /// </summary>
    public async Task Open(ILocator appointment)
    {
        await appointment.ScrollIntoViewIfNeededAsync();
        await appointment.ClickAsync();
    }

    /// <summary>Double-clicks an appointment, brought into view first, as a hand opens it to change it.</summary>
    public async Task OpenToChange(ILocator appointment)
    {
        await appointment.ScrollIntoViewIfNeededAsync();
        await appointment.DblClickAsync();
    }
}
