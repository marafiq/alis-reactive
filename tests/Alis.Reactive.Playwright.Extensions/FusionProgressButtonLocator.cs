using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionProgressButton.
/// </summary>
public sealed class FusionProgressButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionProgressButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered Syncfusion ProgressButton element.</summary>
    public ILocator Button => _page.Locator($"#{_componentId}");

    /// <summary>The rendered Syncfusion content span.</summary>
    public ILocator Content => Button.Locator(".e-btn-content");

    /// <summary>The rendered Syncfusion progress filler span.</summary>
    public ILocator Progress => Button.Locator(".e-progress");

    /// <summary>Gets the rendered button class attribute.</summary>
    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    /// <summary>Returns whether the button currently has the given CSS class.</summary>
    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    /// <summary>Returns whether the button is in Syncfusion's active progress state.</summary>
    public async Task<bool> IsProgressActive() => await HasClass("e-progress-active");
}
