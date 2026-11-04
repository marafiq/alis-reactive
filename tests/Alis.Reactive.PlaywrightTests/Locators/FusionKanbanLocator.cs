using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionKanbanLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionKanbanLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Card(string text) => Root.Locator(".e-card").Filter(new() { HasText = text });

    public ILocator Cell(string columnKey, string swimlaneKey) =>
        Root.Locator($"tr.e-swimlane-row[data-key='{swimlaneKey}'] + tr td.e-content-cells[data-key='{columnKey}']");

    public ILocator CardsIn(ILocator cell) => cell.Locator(".e-card");

    public ILocator ColumnHeader(string headerText) => Root.Locator(".e-header-cells").Filter(new() { HasText = headerText });

    public ILocator ColumnToggle(string headerText) => ColumnHeader(headerText).Locator(".e-header-icon");

    public ILocator Spinner => _page.Locator($"#{_componentId} > .e-spinner-pane");

    public ILocator Dialog => _page.Locator($"#{_componentId}_dialog_wrapper.e-popup-open");

    public ILocator DialogField(string member) => _page.Locator($"#{_componentId}_dialog_wrapper [name='{member}']");

    public ILocator DialogButton(string text) =>
        _page.Locator($"#{_componentId}_dialog_wrapper .e-footer-content button").Filter(new() { HasText = text });

    public async Task PickUpAndCarry(ILocator card, ILocator over, float heightFraction = 0.5f)
    {
        var from = await BoxOf(card);
        var fromX = from.X + from.Width / 2;
        var fromY = from.Y + from.Height / 2;
        await _page.Mouse.MoveAsync(fromX, fromY);
        await _page.Mouse.DownAsync();
        await _page.Mouse.MoveAsync(fromX + 24, fromY + 12, new() { Steps = 8 });

        var to = await BoxOf(over);
        await _page.Mouse.MoveAsync(to.X + to.Width / 2, to.Y + to.Height * heightFraction, new() { Steps = 30 });
    }

    public async Task LetGo() => await _page.Mouse.UpAsync();

    public async Task DragCard(ILocator card, ILocator over, float heightFraction = 0.5f)
    {
        await PickUpAndCarry(card, over, heightFraction);
        await LetGo();
    }

    private static async Task<(float X, float Y, float Width, float Height)> BoxOf(ILocator element)
    {
        await element.ScrollIntoViewIfNeededAsync();
        var box = await element.BoundingBoxAsync()
                  ?? throw new InvalidOperationException($"'{element}' has no box on the page.");
        return (box.X, box.Y, box.Width, box.Height);
    }
}
