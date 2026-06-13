using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator Target => _page.Locator("#resident-context-target");

    public ILocator OpenButton => _page.Locator("#open-context-menu-btn");

    public ILocator CloseButton => _page.Locator("#close-context-menu-btn");

    public ILocator Item(string text) => _page.GetByRole(AriaRole.Menuitem, new() { Name = text });

    /// <summary>
    /// Right-clicks the target and retries once if the menu does not appear —
    /// on slow machines the first right-click can land before EJ2 wires its handler.
    /// </summary>
    public async Task RightClickTarget()
    {
        await Target.ClickAsync(new() { Button = MouseButton.Right });
        try
        {
            await Root.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 2000 });
        }
        catch (TimeoutException)
        {
            await Target.ClickAsync(new() { Button = MouseButton.Right });
        }
    }
}
