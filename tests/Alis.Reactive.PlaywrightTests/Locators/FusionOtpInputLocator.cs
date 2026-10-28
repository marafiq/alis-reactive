using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion OTP Input: one field for each character of the code.</summary>
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

    /// <summary>The field for the character at this zero-based place in the code.</summary>
    public ILocator Field(int index) => Host.Locator("input.e-otp-input-field").Nth(index);

    /// <summary>Types the code into the fields from the first, one character each, as a person keys it in.</summary>
    public async Task TypeCode(string code)
    {
        for (var index = 0; index < code.Length; index++)
        {
            await Field(index).FillAsync(code[index].ToString());
        }
    }
}
