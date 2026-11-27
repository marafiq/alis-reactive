using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffPreparePhotoConsentSheets(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F10.M21_X1305_0)]
    public async Task the_printed_signature_line_uses_the_current_consent_caption()
    {
        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.PhotoConsentSheet);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use plain wording", Exact = true }).ClickAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signature line", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo identification permission", Exact = true }).GetByText("May we use your photo?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature line", Exact = true })).ToHaveTextAsync("May we use your photo?");

        var formal = await NewWorld();
        await formal.Page.GotoAsync(CarePages.PhotoConsentSheetPlain);
        await formal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use formal wording", Exact = true }).ClickAsync();
        await formal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signature line", Exact = true }).ClickAsync();
        await Expect(formal.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo identification permission", Exact = true }).GetByText("I consent to a photo ID", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(formal.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature line", Exact = true })).ToHaveTextAsync("I consent to a photo ID");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1306_0)]
    public async Task the_print_order_matches_the_consent_sheet_size()
    {
        var large = await NewWorld();
        await large.Page.GotoAsync(CarePages.PhotoConsentSheet);
        await large.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use large print", Exact = true }).ClickAsync();
        await large.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(PhotoPermission(large.Page)).ToHaveClassAsync("e-checkbox-wrapper e-wrapper large-print");
        await Expect(large.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print size", Exact = true })).ToHaveTextAsync("Large-print consent packet");

        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.PhotoConsentSheetLarge);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use standard print", Exact = true }).ClickAsync();
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(PhotoPermission(standard.Page)).ToHaveClassAsync("e-checkbox-wrapper e-wrapper");
        await Expect(standard.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print size", Exact = true })).ToHaveTextAsync("Standard consent packet");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1307_0)]
    public async Task the_printed_reading_guide_matches_the_caption_and_direction()
    {
        var arabic = await NewWorld();
        await arabic.Page.GotoAsync(CarePages.PhotoConsentSheetFieldLabel);
        await Expect(arabic.Page.GetByText("Photo permission", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(PhotoPermission(arabic.Page).GetByText("I consent to a photo ID", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await arabic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use Arabic layout", Exact = true }).ClickAsync();
        await arabic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(PhotoPermission(arabic.Page)).ToHaveClassAsync("e-checkbox-wrapper e-wrapper e-rtl");
        await Expect(PhotoPermission(arabic.Page).GetByText("أوافق على استخدام صورتي للهوية", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(arabic.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Reading layout", Exact = true })).ToHaveTextAsync("Arabic layout ready");

        var english = await NewWorld();
        await english.Page.GotoAsync(CarePages.PhotoConsentSheetArabic);
        await Expect(PhotoPermission(english.Page).GetByText("أوافق على استخدام صورتي للهوية", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use English layout", Exact = true }).ClickAsync();
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(PhotoPermission(english.Page)).ToHaveClassAsync("e-checkbox-wrapper e-wrapper");
        await Expect(PhotoPermission(english.Page).GetByText("I consent to a photo ID", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(english.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Reading layout", Exact = true })).ToHaveTextAsync("English layout ready");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1308_0)]
    public async Task the_printed_caption_uses_the_selected_house_or_standard_format()
    {
        var house = await NewWorld();
        await house.Page.GotoAsync(CarePages.PhotoConsentSheet);
        await house.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use house caption", Exact = true }).ClickAsync();
        await house.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(house.Page.GetByText("Signature required", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(house.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Caption format", Exact = true })).ToHaveTextAsync("House-designed caption");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.PhotoConsentSheetHouse);
        await Expect(plain.Page.GetByText("Signature required", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use standard caption", Exact = true }).ClickAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare print order", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByText("Photo release", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Caption format", Exact = true })).ToHaveTextAsync("Standard caption");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1316_0)]
    public async Task a_blank_caption_uses_the_standard_form_until_wording_is_reinstated()
    {
        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.PhotoConsentSheet);
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use field label only", Exact = true }).ClickAsync();
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signature line", Exact = true }).ClickAsync();
        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo identification permission", Exact = true }).GetByText("I consent to a photo ID", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(blank.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature line", Exact = true })).ToHaveTextAsync("Use the standard photo permission form");

        var restored = await NewWorld();
        await restored.Page.GotoAsync(CarePages.PhotoConsentSheetPlain);
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use field label only", Exact = true }).ClickAsync();
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use formal wording", Exact = true }).ClickAsync();
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare signature line", Exact = true }).ClickAsync();
        await Expect(restored.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo identification permission", Exact = true }).GetByText("I consent to a photo ID", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(restored.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature line", Exact = true })).ToHaveTextAsync("I consent to a photo ID");
    }

    private static ILocator PhotoPermission(IPage page)
        => page.GetByLabel("Photo identification permission", new PageGetByLabelOptions { Exact = true })
            .Filter(new LocatorFilterOptions { Has = page.GetByRole(AriaRole.Checkbox) });
}
