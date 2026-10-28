using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// The suggestion list a Syncfusion Mention opens for the input it targets. A Mention with a target renames
/// its list to <c>#{targetId}_popup</c>, after the input the user types in.
/// </summary>
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

    /// <summary>The suggestion whose text is <paramref name="text"/>.</summary>
    public ILocator Item(string text) =>
        Items.Filter(new() { HasTextRegex = new System.Text.RegularExpressions.Regex($"^\\s*{System.Text.RegularExpressions.Regex.Escape(text)}\\s*$") });
}
