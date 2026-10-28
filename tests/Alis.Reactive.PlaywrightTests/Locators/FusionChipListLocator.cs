using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>
/// A Syncfusion ChipList: its chips in order, a chip by its text, the chips it shows selected, and the icon
/// that deletes a chip on a list whose chips can be deleted.
/// </summary>
public sealed class FusionChipListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionChipListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>Every chip, in order.</summary>
    public ILocator Chips => Root.Locator(".e-chip");

    /// <summary>The chip whose text is exactly <paramref name="text"/>.</summary>
    public ILocator Chip(string text) =>
        Chips.Filter(new() { HasTextRegex = new Regex("^" + Regex.Escape(text) + "$") });

    /// <summary>The chips the list shows selected, in order.</summary>
    public ILocator SelectedChips => Root.Locator(".e-chip.e-active");

    /// <summary>The icon that deletes the chip whose text is exactly <paramref name="text"/>.</summary>
    public ILocator DeleteIcon(string text) => Chip(text).Locator(".e-chip-delete");
}
