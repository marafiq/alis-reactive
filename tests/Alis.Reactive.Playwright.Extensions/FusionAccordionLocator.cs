using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates the visible parts of a rendered Syncfusion accordion the way a resident sees them:
/// section headers to click, the expand/collapse state, and the disabled (locked) state.
/// </summary>
public sealed class FusionAccordionLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionAccordionLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The accordion root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The accordion item (panel) at the given zero-based index.</summary>
    public ILocator Item(int index) => Root.Locator(".e-acrdn-item").Nth(index);

    /// <summary>The clickable section header at the given zero-based index.</summary>
    public ILocator Header(int index) => Item(index).Locator(".e-acrdn-header");

    /// <summary>The expanded content body of the panel at the given zero-based index.</summary>
    public ILocator Content(int index) => Item(index).Locator(".e-acrdn-panel .e-acrdn-content");

    /// <summary>Opens the section at the given index the way a resident would — a trusted click on its header.</summary>
    public async Task OpenSection(int index) => await Header(index).ClickAsync();
}
