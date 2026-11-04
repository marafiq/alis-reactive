using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Chips => Root.Locator(".e-chip");

    public ILocator Chip(string text) =>
        Chips.Filter(new() { HasTextRegex = new Regex("^" + Regex.Escape(text) + "$") });

    public ILocator SelectedChips => Root.Locator(".e-chip.e-active");

    public ILocator DeleteIcon(string text) => Chip(text).Locator(".e-chip-delete");
}
