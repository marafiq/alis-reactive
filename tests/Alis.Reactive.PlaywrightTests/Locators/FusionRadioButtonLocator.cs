using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionRadioButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionRadioButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Label => _page.Locator($"label[for='{_componentId}']");

    public async Task Choose() => await Label.ClickAsync();

    public async Task<bool> IsChecked() => await Input.IsCheckedAsync();
}
