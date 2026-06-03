using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locator surface for the Syncfusion OtpInput used by Playwright tests.
/// </summary>
public sealed class FusionOtpInputLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionOtpInputLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Host => _page.Locator($"#{_componentId}");

    /// <summary>The hidden input Syncfusion uses as the submitted value.</summary>
    public ILocator HiddenInput => Host.Locator("input[type='hidden']");

    /// <summary>Gets a visible OTP field by zero-based index.</summary>
    public ILocator Field(int index) => Host.Locator("input.e-otp-input-field").Nth(index);

    public async Task<string> HiddenValue() => await HiddenInput.InputValueAsync();

    public async Task<string> FieldValue(int index) => await Field(index).InputValueAsync();

    public async Task FillCode(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            await Field(index).FillAsync(value[index].ToString());
        }
    }

    public async Task Focus() => await Field(0).ClickAsync();
}
