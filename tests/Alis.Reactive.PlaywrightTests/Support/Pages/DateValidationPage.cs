using System;
using Microsoft.Playwright;
using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Support.Pages;

internal sealed class DateValidationPage
{
    private const string Path = "/Sandbox/Validation/DateRules";
    private const string Prefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateValidationModel__";

    private readonly IPage _page;
    private readonly Func<Task> _open;

    internal DateValidationPage(IPage page, Func<Task> open)
    {
        _page = page;
        _open = open;
    }

    internal Task Open() => _open();

    internal ILocator ValidateButton => _page.Locator("#validate-dates-btn");
    internal ILocator Result => _page.Locator("#date-result");

    internal ILocator ErrorFor(string suffix) => _page.Locator($"#{Prefix}{suffix}_error");

    internal DatePickerLocator DatePicker(string suffix) => new(_page, Prefix + suffix);
}
