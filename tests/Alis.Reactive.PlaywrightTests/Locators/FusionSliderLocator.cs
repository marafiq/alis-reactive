using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionSliderLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSliderLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Host => _page.Locator($"#{_componentId}");

    public ILocator Handle(int index = 0) => Host.Locator(".e-handle").Nth(index);

    public async Task<string?> ValueNow(int index = 0) =>
        await Handle(index).GetAttributeAsync("aria-valuenow");

    public async Task NudgeUp(int index = 0)
    {
        await Handle(index).FocusAsync();
        await _page.Keyboard.PressAsync("ArrowRight");
    }
}
