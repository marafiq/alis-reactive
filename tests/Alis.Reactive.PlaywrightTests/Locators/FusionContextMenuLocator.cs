using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionContextMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionContextMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Lists => Root.Locator("xpath=..");

    public ILocator Item(string text)
        => Lists.GetByRole(AriaRole.Menuitem).Filter(new() { HasTextRegex = new Regex($@"^\s*{Regex.Escape(text)}\s*$") });
}
