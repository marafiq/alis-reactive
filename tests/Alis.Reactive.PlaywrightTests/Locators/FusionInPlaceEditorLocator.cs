using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionInPlaceEditorLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionInPlaceEditorLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator ShownValue => Root.Locator(".e-editable-value");

    public ILocator ValueArea => Root.Locator(".e-editable-value-wrapper");

    public ILocator ActionButtons => Root.Locator(".e-editable-action-buttons");

    public ILocator Input => Root.Locator(".e-editable-component input.e-control").First;

    public ILocator ListInput => Root.Locator(".e-editable-component .e-input-group").First;

    public ILocator SaveButton => Root.Locator(".e-btn-save");

    public ILocator CancelButton => Root.Locator(".e-btn-cancel");

    public ILocator ListItem(string text)
        => _page.Locator(".e-popup-open li.e-list-item").Filter(new() { HasTextString = text });

    public async Task Open()
    {
        await ValueArea.ClickAsync();
        await ActionButtons.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    public async Task OpenAndType(string value)
    {
        await Open();
        await Input.FillAsync(value);
    }
}
