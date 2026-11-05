using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class NativeRadioGroupLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal NativeRadioGroupLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    private ILocator Group => _page.Locator($"#{_componentId}").Locator("xpath=..");

    private ILocator Label(string optionText)
        => Group.Locator("label").Filter(new() { Has = _page.GetByText(optionText, new() { Exact = true }) });

    public ILocator Option(string optionText) => Label(optionText).Locator("input[type=radio]");

    public ILocator Box(string optionText) => Label(optionText).Locator("> div");

    public async Task Choose(string optionText) => await Label(optionText).ClickAsync();
}
