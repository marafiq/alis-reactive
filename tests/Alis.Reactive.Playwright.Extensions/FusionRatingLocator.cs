using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionRatingLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionRatingLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public async Task<string?> ValueAttribute() =>
        await Input.GetAttributeAsync("value");

    /// <summary>The visible rating list's ARIA value.</summary>
    public async Task<string?> AriaValue() =>
        await _page.EvaluateAsync<string?>(
            @"id => {
                const el = document.getElementById(id);
                const root = el?.parentElement ?? document;
                const list = root.querySelector('.e-rating-item-list') ?? document.querySelector('.e-rating-item-list');
                return list ? list.getAttribute('aria-valuenow') : null;
            }",
            _componentId);
}
