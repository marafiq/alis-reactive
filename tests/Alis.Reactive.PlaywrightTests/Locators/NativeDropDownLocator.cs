using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class NativeDropDownLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeDropDownLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task Select(string text)
        => await Input.SelectOptionAsync(new SelectOptionValue { Label = text });
}
