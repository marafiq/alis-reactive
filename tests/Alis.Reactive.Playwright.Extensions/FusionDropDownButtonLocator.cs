using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionDropDownButton.
/// </summary>
public sealed class FusionDropDownButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered Syncfusion DropDownButton element.</summary>
    public ILocator Button => _page.Locator($"#{_componentId}");

    /// <summary>The rendered Syncfusion popup element.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}-popup");

    /// <summary>The rendered popup action items.</summary>
    public ILocator Items => Popup.Locator("li.e-item");

    /// <summary>Locates a popup action item by exact visible text.</summary>
    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    /// <summary>Gets the rendered button class attribute.</summary>
    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Gets the rendered popup class attribute.</summary>
    public async Task<string> PopupClassAttribute() =>
        await Popup.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Returns whether the button currently has the given CSS class.</summary>
    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
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
