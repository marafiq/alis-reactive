using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a <c>NativeCheckList</c>. The component id is the list's container; each option is a labelled
/// checkbox inside it, found by the option text a user reads (not its description), scoped to this list.
/// </summary>
public sealed class NativeCheckListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public NativeCheckListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    private ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The checkbox of the option whose text is exactly <paramref name="optionText"/>.</summary>
    public ILocator Box(string optionText) =>
        Root.Locator("label")
            .Filter(new() { Has = _page.GetByText(optionText, new() { Exact = true }) })
            .Locator("input[type=checkbox]");

    /// <summary>Ticks the option whose text is exactly <paramref name="optionText"/>.</summary>
    public async Task Tick(string optionText) => await Box(optionText).CheckAsync();

    /// <summary>Clears the option whose text is exactly <paramref name="optionText"/>.</summary>
    public async Task Clear(string optionText) => await Box(optionText).UncheckAsync();
}
