using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>
/// A Syncfusion AIAssistView: the box a prompt is typed into, the suggestions it offers, the conversation's
/// prompts and responses, and the button that stops a response.
/// </summary>
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

    /// <summary>The box a prompt is typed into; Enter sends it.</summary>
    public ILocator PromptBox => Root.Locator(".e-assist-textarea");

    /// <summary>The suggestion with this text, which sends itself when clicked.</summary>
    public ILocator Suggestion(string text) => Root.Locator(".e-suggestions li").Filter(new() { HasText = text });

    /// <summary>Every prompt in the conversation, in order.</summary>
    public ILocator Prompts => Root.Locator(".e-prompt-text");

    /// <summary>Every response in the conversation, in order.</summary>
    public ILocator Responses => Root.Locator(".e-content-body");

    /// <summary>The button that stops the response being prepared; it shows in place of send until the response is final.</summary>
    public ILocator StopResponding => Root.Locator(".e-assist-stop");
}
