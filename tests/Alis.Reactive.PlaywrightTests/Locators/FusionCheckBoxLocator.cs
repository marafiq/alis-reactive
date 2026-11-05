using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionCheckBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionCheckBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-checkbox-wrapper ')][1]");

    public ILocator Frame => Wrapper.Locator(".e-frame");

    public ILocator Label => Wrapper.Locator(".e-label");

    public async Task Toggle() => await Frame.ClickAsync();

    private async Task<bool> FrameHasClass(string className)
    {
        var classes = await Frame.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }

    private async Task<bool> WrapperHasClass(string className)
    {
        var classes = await Wrapper.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ').Contains(className);
    }

    public Task<bool> IsChecked() => FrameHasClass("e-check");

    public Task<bool> IsIndeterminate() => FrameHasClass("e-stop");

    public Task<bool> IsDisabled() => WrapperHasClass("e-checkbox-disabled");
}
