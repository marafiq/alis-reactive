using System;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Pages;

internal sealed class SpecializedValidationPage
{
    private const string Path = "/Sandbox/Validation/SpecializedRules";
    private const string Prefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NewRuleTypesModel__";

    private readonly IPage _page;
    private readonly Func<Task> _open;

    internal SpecializedValidationPage(IPage page, Func<Task> open)
    {
        _page = page;
        _open = open;
    }

    internal Task Open() => _open();

    internal ILocator ValidateButton => _page.Locator("#validate-new-rules-btn");
    internal ILocator Result => _page.Locator("#new-rules-result");
    internal ILocator Input(string prop) => _page.Locator($"#{Prefix}{prop}");
    internal ILocator ErrorFor(string prop) => _page.Locator($"#{Prefix}{prop}_error");
}
