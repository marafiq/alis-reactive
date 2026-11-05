using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Field(int index) => Host.Locator("input.e-otp-input-field").Nth(index);

    public async Task TypeCode(string code)
    {
        for (var index = 0; index < code.Length; index++)
        {
            await Field(index).FillAsync(code[index].ToString());
        }
    }
}
