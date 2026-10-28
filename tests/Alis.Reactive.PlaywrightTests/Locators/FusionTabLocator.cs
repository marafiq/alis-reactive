using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates the visible parts of a rendered Syncfusion tab the way a user sees them:
/// the section headers to click, which header is active, the active section's content,
/// and how many sections the tab strip currently offers.
/// </summary>
public sealed class FusionTabLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionTabLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The tab root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The clickable section header at the given zero-based index.</summary>
    public ILocator Header(int index) =>
        Root.Locator(".e-tab-header .e-toolbar-item").Nth(index);

    /// <summary>The currently active (selected) section header.</summary>
    public ILocator ActiveHeader =>
        Root.Locator(".e-tab-header .e-toolbar-item.e-active");

    /// <summary>The content panel that is currently displayed.</summary>
    public ILocator ActiveContent =>
        Root.Locator(".e-content > .e-item.e-active");

    /// <summary>The section headers a user can actually see — hidden sections are excluded.</summary>
    public ILocator VisibleHeaders =>
        Root.Locator(".e-tab-header .e-toolbar-item:not(.e-hidden)");

    /// <summary>The section header carrying the given label (for example "Billing").</summary>
    public ILocator HeaderByText(string label) =>
        Root.Locator(".e-tab-header .e-toolbar-item").Filter(new() { HasTextString = label });

    /// <summary>Opens the section at the given index the way a user would — a trusted click on its header.</summary>
    public async Task OpenSection(int index) => await Header(index).ClickAsync();
}
