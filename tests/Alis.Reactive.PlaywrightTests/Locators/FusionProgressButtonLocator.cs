using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion ProgressButton: the button, the words on it, and the bar that fills while its progress runs.</summary>
public sealed class FusionProgressButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionProgressButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Button => _page.Locator($"#{_componentId}");

    /// <summary>The words on the button.</summary>
    public ILocator Content => Button.Locator(".e-btn-content");

    /// <summary>The bar a button that shows progress fills as it runs; a button that only spins has none.</summary>
    public ILocator ProgressBar => Button.Locator(".e-progress");
}
