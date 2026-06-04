using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator Content => Button.Locator(".e-btn-content");

    public ILocator Progress => Button.Locator(".e-progress");

    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<bool> IsProgressActive() => await HasClass("e-progress-active");
}
