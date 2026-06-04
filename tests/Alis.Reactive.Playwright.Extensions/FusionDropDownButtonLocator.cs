using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionDropDownButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Button => _page.Locator($"#{_componentId}");

    public ILocator Popup => _page.Locator($"#{_componentId}-popup");

    public ILocator Items => Popup.Locator("li.e-item");

    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    public async Task<string> PopupClassAttribute() =>
        await Popup.GetAttributeAsync("class") ?? string.Empty;

    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> PopupHasClass(string className)
    {
        var classes = await PopupClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> IsPopupOpen() => await PopupHasClass("e-popup-open");
}
