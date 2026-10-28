using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a native <c>&lt;select&gt;</c> rendered by <c>NativeDropDown</c>; the component id is
/// the select's own id, so choosing an option is the browser's own select gesture.
/// </summary>
public sealed class NativeDropDownLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeDropDownLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>Selects the option whose visible text is exactly <paramref name="text"/>.</summary>
    public async Task Select(string text) =>
        await Input.SelectOptionAsync(new SelectOptionValue { Label = text });
}
