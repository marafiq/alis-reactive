using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion InPlaceEditor rendered inline: a shown value that opens an editor with save and cancel buttons.</summary>
public sealed class FusionInPlaceEditorLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionInPlaceEditorLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The editor's root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The value shown while the editor is closed ("Empty" when there is none).</summary>
    public ILocator ShownValue => Root.Locator(".e-editable-value");

    /// <summary>The area a click opens the editor from.</summary>
    public ILocator ValueArea => Root.Locator(".e-editable-value-wrapper");

    /// <summary>The save and cancel buttons, present only while the editor is open.</summary>
    public ILocator ActionButtons => Root.Locator(".e-editable-action-buttons");

    /// <summary>The open editor's text input.</summary>
    public ILocator Input => Root.Locator(".e-editable-component input.e-control").First;

    /// <summary>The open list editor's input, which opens its list when clicked.</summary>
    public ILocator ListInput => Root.Locator(".e-editable-component .e-input-group").First;

    public ILocator SaveButton => Root.Locator(".e-btn-save");

    public ILocator CancelButton => Root.Locator(".e-btn-cancel");

    /// <summary>An item of the open list editor's popup.</summary>
    public ILocator ListItem(string text) =>
        _page.Locator(".e-popup-open li.e-list-item").Filter(new() { HasTextString = text });

    /// <summary>Clicks the shown value and waits for the editor to open.</summary>
    public async Task Open()
    {
        await ValueArea.ClickAsync();
        await ActionButtons.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>Opens the editor and types over its value.</summary>
    public async Task OpenAndType(string value)
    {
        await Open();
        await Input.FillAsync(value);
    }
}
