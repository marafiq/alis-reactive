using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// A Syncfusion Kanban: its cards, the cell of a column in a swimlane, its column headers, its card dialog
/// and its spinner, and a card dragged by the pointer the way a hand drags it.
/// </summary>
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

    /// <summary>The card that shows this text.</summary>
    public ILocator Card(string text) => Root.Locator(".e-card").Filter(new() { HasText = text });

    /// <summary>The cell of a column in a swimlane, by the column's key and the swimlane's key.</summary>
    public ILocator Cell(string columnKey, string swimlaneKey) =>
        Root.Locator($"tr.e-swimlane-row[data-key='{swimlaneKey}'] + tr td.e-content-cells[data-key='{columnKey}']");

    /// <summary>The cards of a cell, in the order the board shows them.</summary>
    public ILocator CardsIn(ILocator cell) => cell.Locator(".e-card");

    /// <summary>The header of the column that shows this text.</summary>
    public ILocator ColumnHeader(string headerText) => Root.Locator(".e-header-cells").Filter(new() { HasText = headerText });

    /// <summary>The icon that folds and unfolds a column the user may fold.</summary>
    public ILocator ColumnToggle(string headerText) => ColumnHeader(headerText).Locator(".e-header-icon");

    /// <summary>The board's spinner; it carries <c>e-spin-show</c> while it shows.</summary>
    public ILocator Spinner => _page.Locator($"#{_componentId} > .e-spinner-pane");

    /// <summary>The card dialog while it is open.</summary>
    public ILocator Dialog => _page.Locator($"#{_componentId}_dialog_wrapper.e-popup-open");

    /// <summary>A field of the card dialog, by the card member it edits.</summary>
    public ILocator DialogField(string member) => _page.Locator($"#{_componentId}_dialog_wrapper [name='{member}']");

    /// <summary>A button of the card dialog, by its text.</summary>
    public ILocator DialogButton(string text) =>
        _page.Locator($"#{_componentId}_dialog_wrapper .e-footer-content button").Filter(new() { HasText = text });

    /// <summary>
    /// Presses on a card and carries it over a target, a cell or another card, without letting go: the
    /// pointer stops at the given fraction of the target's height, so over a card's lower half the card
    /// is dropped after it.
    /// </summary>
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

    /// <summary>Lets go of a card being carried.</summary>
    public async Task LetGo() => await _page.Mouse.UpAsync();

    /// <summary>Drags a card over a target and drops it there.</summary>
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
