using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionMentionLocator
{
    private readonly IPage _page;
    private readonly string _targetId;

    public FusionMentionLocator(IPage page, string targetId)
    {
        _page = page;
        _targetId = targetId;
    }

    public ILocator Popup => _page.Locator($"#{_targetId}_popup");

    public ILocator Items => Popup.Locator(".e-list-item");

    public ILocator Item(string text) =>
        Items.Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") });
}
