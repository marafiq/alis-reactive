using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a native checkbox rendered by <c>NativeCheckBox</c>; the component id is the
/// checkbox's own id, so ticking it is the browser's own check gesture.
/// </summary>
public sealed class NativeCheckBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeCheckBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>Ticks the checkbox.</summary>
    public async Task Check() => await Input.CheckAsync();
}
