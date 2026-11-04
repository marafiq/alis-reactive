using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class SwitchLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal SwitchLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Wrapper => _page.Locator($".e-switch-wrapper:has(#{_componentId})");

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Toggle() => await Wrapper.ClickWhenStableAsync(_page);
}
