using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Box(string optionText) =>
        Root.Locator("label")
            .Filter(new() { Has = _page.GetByText(optionText, new() { Exact = true }) })
            .Locator("input[type=checkbox]");

    public async Task Tick(string optionText) => await Box(optionText).CheckAsync();

    public async Task Clear(string optionText) => await Box(optionText).UncheckAsync();
}
