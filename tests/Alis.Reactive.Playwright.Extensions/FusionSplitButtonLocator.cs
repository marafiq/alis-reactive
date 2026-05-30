using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionSplitButton.
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

    /// <summary>The rendered Syncfusion SplitButton primary button element.</summary>
    public ILocator Primary => _page.Locator($"#{_componentId}");

    /// <summary>The rendered Syncfusion SplitButton secondary dropdown button element.</summary>
    public ILocator Secondary => _page.Locator($"#{_componentId}_dropdownbtn");

    /// <summary>The rendered Syncfusion SplitButton wrapper.</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::*[contains(concat(' ', normalize-space(@class), ' '), ' e-split-btn-wrapper ')][1]");

    /// <summary>The rendered Syncfusion popup element.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_dropdownbtn-popup");

    /// <summary>The rendered popup action items.</summary>
    public ILocator Items => Popup.Locator("li.e-item");

    /// <summary>Locates a popup action item by exact visible text.</summary>
    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}_dropdownbtn-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    /// <summary>Gets the rendered primary button class attribute.</summary>
    public async Task<string> PrimaryClassAttribute() =>
        await Primary.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Gets the rendered secondary button class attribute.</summary>
    public async Task<string> SecondaryClassAttribute() =>
        await Secondary.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Gets the rendered wrapper class attribute.</summary>
    public async Task<string> WrapperClassAttribute() =>
        await Wrapper.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Gets the rendered popup class attribute.</summary>
    public async Task<string> PopupClassAttribute() =>
        await Popup.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Returns whether the primary button currently has the given CSS class.</summary>
    public async Task<bool> PrimaryHasClass(string className)
    {
        var classes = await PrimaryClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Returns whether the secondary button currently has the given CSS class.</summary>
    public async Task<bool> SecondaryHasClass(string className)
    {
        var classes = await SecondaryClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Returns whether the wrapper currently has the given CSS class.</summary>
    public async Task<bool> WrapperHasClass(string className)
    {
        var classes = await WrapperClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Returns whether the popup currently has the given CSS class.</summary>
    public async Task<bool> PopupHasClass(string className)
    {
        var classes = await PopupClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Returns whether the popup is currently open.</summary>
    public async Task<bool> IsPopupOpen() => await PopupHasClass("e-popup-open");
}
