using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Button => _page.Locator($"#{_componentId}");

    public ILocator Icon => Button.Locator("span.e-btn-icon");

    public async Task<string> ClassAttribute() =>
        await Button.GetAttributeAsync("class") ?? string.Empty;

    public async Task<bool> HasClass(string className)
    {
        var classes = await ClassAttribute();
        return classes.Split(' ').Contains(className);
    }

    public async Task<string> IconClassAttribute() =>
        await Icon.GetAttributeAsync("class") ?? string.Empty;
}
