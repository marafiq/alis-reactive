using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class SwitchLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal SwitchLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>Clickable wrapper; the component ID belongs to the hidden checkbox.</summary>
    public ILocator Wrapper => _page.Locator($".e-switch-wrapper:has(#{_componentId})");

    /// <summary>Hidden checkbox input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Toggle() => await Wrapper.ClickWhenStableAsync(_page);
}
