using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a <c>NativeRadioGroup</c>. The component id is the group's hidden value input; its
/// options are the labelled radios beside it, so <see cref="Choose"/> clicks the option label a
/// user reads, scoped to this group.
/// </summary>
public sealed class NativeRadioGroupLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeRadioGroupLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    private ILocator Group => _page.Locator($"#{_componentId}").Locator("xpath=..");

    /// <summary>Clicks the option whose text is exactly <paramref name="optionText"/>.</summary>
    public async Task Choose(string optionText) =>
        await Group.Locator("label")
            .Filter(new() { Has = _page.GetByText(optionText, new() { Exact = true }) })
            .ClickAsync();
}
