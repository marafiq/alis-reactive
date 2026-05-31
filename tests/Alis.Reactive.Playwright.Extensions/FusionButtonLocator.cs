using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionButton.
/// </summary>
public sealed class FusionButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered Syncfusion Button element.</summary>
    public ILocator Button => _page.Locator($"#{_componentId}");

    /// <summary>The rendered Syncfusion icon span.</summary>
    public ILocator Icon => Button.Locator("span.e-btn-icon");

    /// <summary>Gets the rendered button class attribute.</summary>
    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Returns whether the button currently has the given CSS class.</summary>
    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Gets the rendered icon class attribute.</summary>
    public async Task<string> IconClassAttribute() =>
        await Icon.GetAttributeAsync("class") ?? string.Empty;
}
