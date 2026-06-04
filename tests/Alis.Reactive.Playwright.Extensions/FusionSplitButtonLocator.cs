using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright gestures and surfaces for FusionSplitButton tests.
/// </summary>
public sealed class FusionSplitButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSplitButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Primary => _page.Locator($"#{_componentId}");

    public ILocator Secondary => _page.Locator($"#{_componentId}_dropdownbtn");

    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-split-btn-wrapper ')][1]");

    public ILocator Popup => _page.Locator($"#{_componentId}_dropdownbtn-popup");

    public ILocator Items => Popup.Locator("li.e-item");

    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}_dropdownbtn-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    public async Task<string> PrimaryClassAttribute() =>
        await Primary.GetAttributeAsync("class") ?? string.Empty;

    public async Task<string> SecondaryClassAttribute() =>
        await Secondary.GetAttributeAsync("class") ?? string.Empty;

    public async Task<string> WrapperClassAttribute() =>
        await Wrapper.GetAttributeAsync("class") ?? string.Empty;

    public async Task<string> PopupClassAttribute() =>
        await Popup.GetAttributeAsync("class") ?? string.Empty;

    public async Task<bool> PrimaryHasClass(string className)
    {
        var classes = await PrimaryClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> SecondaryHasClass(string className)
    {
        var classes = await SecondaryClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> WrapperHasClass(string className)
    {
        var classes = await WrapperClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> PopupHasClass(string className)
    {
        var classes = await PopupClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> IsPopupOpen() => await PopupHasClass("e-popup-open");
}
