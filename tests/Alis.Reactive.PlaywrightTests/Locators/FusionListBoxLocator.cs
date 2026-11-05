using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionListBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}_parent");

    public ILocator Item(string text) => Root
        .Locator("li.e-list-item")
        .Filter(new() { HasTextString = text })
        .First;

    public ILocator SelectedItems => Root.Locator("li.e-list-item.e-selected");

    public async Task ClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
    }

    public async Task CtrlClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickAsync(new() { Modifiers = new[] { KeyboardModifier.ControlOrMeta }, Timeout = timeoutMs });
    }
}
