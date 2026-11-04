using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionListViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Item(string text) =>
        Root.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::li[contains(@class,'e-list-item')][1]");

    public ILocator SelectedItems => Root.Locator("li.e-list-item.e-active");

    public ILocator CheckMark(string text) => Item(text).Locator(".e-check");

    public ILocator CheckMarks => Root.Locator("li.e-list-item .e-check");

    public async Task ClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
    }
}
