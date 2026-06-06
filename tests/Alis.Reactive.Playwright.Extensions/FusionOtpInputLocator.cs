using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    /// <summary>Hidden input Syncfusion submits as the component value.</summary>
    public ILocator HiddenInput => Host.Locator("input[type='hidden']");

    /// <summary>Visible OTP field at zero-based index.</summary>
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
