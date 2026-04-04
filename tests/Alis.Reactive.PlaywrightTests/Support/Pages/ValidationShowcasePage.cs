using System;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Pages;

internal sealed class ValidationShowcasePage
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string Prefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private readonly IPage _page;
    private readonly Func<Task> _open;

    internal ValidationShowcasePage(IPage page, Func<Task> open)
    {
        _page = page;
        _open = open;
    }

    internal Task Open() => _open();

    internal ILocator ValidateAllRulesButton => _page.Locator("#validate-all-btn");
    internal ILocator AllRulesResult => _page.Locator("#all-rules-result");
    internal ILocator ValidateConditionalButton => _page.Locator("#conditional-validate-btn");
    internal ILocator ConditionalResult => _page.Locator("#conditional-result");
    internal ILocator ValidateServerButton => _page.Locator("#server-save-btn");
    internal ILocator ValidateHiddenButton => _page.Locator("#hidden-validate-btn");
    internal ILocator HiddenResult => _page.Locator("#hidden-result");
    internal ILocator ValidateDatabaseButton => _page.Locator("#db-save-btn");
    internal ILocator DatabaseResult => _page.Locator("#db-result");
    internal ILocator ServerResult => _page.Locator("#server-result");
    internal ILocator HiddenExtras => _page.Locator("#hf_extras");

    internal ILocator Input(string suffix) => _page.Locator($"#{Prefix}{suffix}");

    internal ILocator ErrorFor(string suffix) => _page.Locator($"#{Prefix}{suffix}_error");
}
