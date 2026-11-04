using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class NativeCheckBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeCheckBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Check() => await Input.CheckAsync();
}
