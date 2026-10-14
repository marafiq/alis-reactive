using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ColorPicker;

/// <summary>
/// As a community's branding coordinator, I choose the brand and accent colors, am warned when the
/// brand color would hide its white text, and review the colors before they go out.
/// </summary>
[TestFixture]
public class WhenColorPicked : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ColorPicker";

    private static readonly string BrandId = IdGenerator.For<ColorPickerModel>(m => m.ThemeColor);
    private static readonly string AccentId = IdGenerator.For<ColorPickerModel>(m => m.AccentColor);

    // Each picker's popup lives in the body; only the one being used is open.
    private ILocator OpenPicker => Page.Locator(".e-colorpicker-popup.e-popup-open");

    // The picker's own arrow button: the split button's secondary button, "<id>_dropdownbtn".
    private ILocator ArrowOf(string pickerId) =>
        Page.Locator($"#{pickerId}").Locator("xpath=..").Locator("button[id$='_dropdownbtn']");

    private Task OpenBranding() => NavigateToAndWaitForBoot(Path);

    private async Task ApplyBrandColor(string hex)
    {
        await ArrowOf(BrandId).ClickAsync();
        await Expect(OpenPicker).ToBeVisibleAsync(new() { Timeout = 5000 });
        await OpenPicker.Locator(".e-hex").FillAsync(hex);
        await OpenPicker.Locator(".e-apply").ClickAsync();
        await Expect(OpenPicker).ToHaveCountAsync(0, new() { Timeout = 5000 });
    }

    private async Task ChooseAccentSwatch(string swatch)
    {
        await ArrowOf(AccentId).ClickAsync();
        await Expect(OpenPicker).ToBeVisibleAsync(new() { Timeout = 5000 });
        await OpenPicker.Locator(swatch).ClickAsync();
        await Expect(OpenPicker).ToHaveCountAsync(0, new() { Timeout = 5000 });
    }

    [Test]
    public async Task choosing_the_brand_color_opens_its_picker_and_choosing_again_closes_it()
    {
        await OpenBranding();

        await Page.Locator("#choose-brand-color").ClickAsync();

        await Expect(OpenPicker).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(OpenPicker.Locator(".e-hex")).ToHaveValueAsync("#3b82f6");

        // Enter presses the button without a mouse-down outside the picker, which would close it on its own.
        await Page.Locator("#choose-brand-color").PressAsync("Enter");

        await Expect(OpenPicker).ToHaveCountAsync(0, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_white_brand_color_warns_that_its_white_text_cannot_be_read()
    {
        await OpenBranding();
        var warning = Page.Locator("#brand-contrast-warning");

        await ApplyBrandColor("#ffffff");

        await Expect(warning).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(warning).ToHaveTextAsync(
            "White text on a white brand color cannot be read. Choose a darker brand color.");

        await ApplyBrandColor("#0f766e");

        await Expect(warning).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_branding_shows_the_colors_chosen()
    {
        await OpenBranding();

        await ChooseAccentSwatch(".e-tile[aria-label='#009688ff']");
        await Page.Locator("#review-branding").ClickAsync();

        await Expect(Page.Locator("#branding-review")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#reviewed-brand")).ToHaveTextAsync("#3b82f6ff");
        await Expect(Page.Locator("#reviewed-accent")).ToHaveTextAsync("#009688ff");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_branding_with_no_accent_color_says_none()
    {
        await OpenBranding();

        await ChooseAccentSwatch(".e-tile.e-nocolor-item");
        await Page.Locator("#review-branding").ClickAsync();

        await Expect(Page.Locator("#branding-review")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#reviewed-accent")).ToHaveTextAsync("None");
        await Expect(Page.Locator("#reviewed-brand")).ToHaveTextAsync("#3b82f6ff");
        AssertNoConsoleErrors();
    }
}
