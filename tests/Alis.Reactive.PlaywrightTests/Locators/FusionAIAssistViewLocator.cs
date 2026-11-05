using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionAIAssistViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionAIAssistViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator PromptBox => Root.Locator(".e-assist-textarea");

    public ILocator Suggestion(string text) => Root.Locator(".e-suggestions li").Filter(new() { HasText = text });

    public ILocator Prompts => Root.Locator(".e-prompt-text");

    public ILocator Responses => Root.Locator(".e-content-body");

    public ILocator StopResponding => Root.Locator(".e-assist-stop");
}
