using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenLaundryStaffPrintScaleSlips(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1618_0)]
    public async Task print_columns_use_the_selected_width_and_reading_direction()
    {
        var narrow = await NewWorld();
        await narrow.Page.GotoAsync(CarePages.LinenScaleSheet);
        await narrow.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print narrow Arabic column", Exact = true }).ClickAsync();
        await Expect(narrow.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 180px;");
        await Expect(narrow.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print layout", Exact = true })).ToHaveTextAsync("Right-to-left scale column");

        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.LinenScaleSheet);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print narrow Arabic column", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print standard column", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 75%;");
        await Expect(wide.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print layout", Exact = true })).ToHaveTextAsync("Standard scale column");
        await Expect(narrow.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-control-wrapper e-numeric e-input-group e-prepend-wrapper e-append-wrapper compact-scale e-rtl");
        await Expect(wide.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-control-wrapper e-numeric e-input-group e-prepend-wrapper e-append-wrapper standard-scale");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1619_0)]
    public async Task leaving_a_scale_column_width_unspecified_keeps_the_selected_print_width()
    {
        var narrow = await NewWorld();
        await narrow.Page.GotoAsync(CarePages.LinenScaleSheet);
        await narrow.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print narrow Arabic column", Exact = true }).ClickAsync();
        await narrow.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep narrow width", Exact = true }).ClickAsync();
        await Expect(narrow.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 180px;");

        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.LinenScaleSheet);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print standard column", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep standard width", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByText("Scale Kilograms", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 75%;");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1620_0)]
    public async Task the_weighing_slip_can_keep_raise_or_inline_its_caption()
    {
        var permanent = await NewWorld();
        await permanent.Page.GotoAsync(CarePages.LinenScaleSheet);
        await permanent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await Expect(permanent.Page.GetByText("Weight to record", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");
        await Expect(permanent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed caption", Exact = true })).ToHaveTextAsync("Permanent weight caption");

        var floating = await NewWorld();
        await floating.Page.GotoAsync(CarePages.LinenScaleSheet);
        await floating.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise caption on focus", Exact = true }).ClickAsync();
        await Expect(floating.Page.GetByText("Weight to record", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-bottom");
        await Expect(floating.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed caption", Exact = true })).ToHaveTextAsync("Focus-raised weight caption");

        var inline = await NewWorld();
        await inline.Page.GotoAsync(CarePages.LinenScaleSheet);
        await inline.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await inline.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use inline caption", Exact = true }).ClickAsync();
        await Expect(inline.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("placeholder", "Weight to record");
        await Expect(inline.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed caption", Exact = true })).ToHaveTextAsync("Inline weight caption");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1621_0)]
    public async Task community_and_ward_scales_use_their_selected_weighing_instructions()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.LinenScaleSheet);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use ward instructions", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("placeholder", "Weigh without the cart");
        await Expect(ward.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Weighing instructions", Exact = true })).ToHaveTextAsync("Ward weighing instructions");

        var community = await NewWorld();
        await community.Page.GotoAsync(CarePages.LinenScaleSheet);
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use ward instructions", Exact = true }).ClickAsync();
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use community language", Exact = true }).ClickAsync();
        await Expect(community.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("placeholder", "Weight to record");
        await Expect(community.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Weighing instructions", Exact = true })).ToHaveTextAsync("Community weighing instructions");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1622_0)]
    public async Task the_printed_guide_distinguishes_no_local_wording_from_suppressed_wording()
    {
        var handbook = await NewWorld();
        await handbook.Page.GotoAsync(CarePages.LinenScaleSheet);
        await handbook.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove local wording", Exact = true }).ClickAsync();
        await Expect(handbook.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Weighing guide", Exact = true })).ToHaveTextAsync("Use the ward weighing handbook");

        var diagram = await NewWorld();
        await diagram.Page.GotoAsync(CarePages.LinenScaleSheet);
        await diagram.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Suppress wording", Exact = true }).ClickAsync();
        await Expect(diagram.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Weighing guide", Exact = true })).ToHaveTextAsync("Use the scale diagram");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1623_0)]
    public async Task ward_receiving_and_replacement_slips_carry_their_own_source_and_units()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.LinenScaleSheet);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare delivery slip", Exact = true }).ClickAsync();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare ward slip", Exact = true }).ClickAsync();
        await Expect(PrintedSlip(ward.Page).Nth(0)).ToHaveTextAsync("Ward scale");
        await Expect(PrintedSlip(ward.Page).Nth(2)).ToHaveTextAsync("Kilograms");

        var delivery = await NewWorld();
        await delivery.Page.GotoAsync(CarePages.LinenScaleSheet);
        await delivery.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare delivery slip", Exact = true }).ClickAsync();
        await Expect(PrintedSlip(delivery.Page).Nth(0)).ToHaveTextAsync("Receiving scale");
        await Expect(PrintedSlip(delivery.Page).Nth(2)).ToHaveTextAsync("Pounds");
        await delivery.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use plain replacement slip", Exact = true }).ClickAsync();
        await Expect(delivery.Page.GetByText("Receiving scale", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(delivery.Page.GetByText("Pounds", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(delivery.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Replacement slip", Exact = true })).ToHaveTextAsync("Write the unit on the slip");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1624_0)]
    public async Task live_ward_and_receiving_labels_follow_the_scale_being_printed()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.LinenScaleSheet);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Attach ward labels", Exact = true }).ClickAsync();
        await Expect(PrintedSlip(ward.Page).Nth(0)).ToHaveTextAsync("Ward laundry");
        await Expect(PrintedSlip(ward.Page).Nth(2)).ToHaveTextAsync("Net kilograms");

        var delivery = await NewWorld();
        await delivery.Page.GotoAsync(CarePages.LinenScaleSheet);
        await delivery.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Attach receiving labels", Exact = true }).ClickAsync();
        await Expect(PrintedSlip(delivery.Page).Nth(0)).ToHaveTextAsync("Supplier delivery");
        await Expect(PrintedSlip(delivery.Page).Nth(2)).ToHaveTextAsync("Gross pounds");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1625_0)]
    public async Task signed_returns_carry_the_zero_or_missing_count_through_the_native_form()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.LinenStocktake);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign stocktake return", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signed return", Exact = true })).ToHaveTextAsync("Signed empty cart");

        var pending = await NewWorld();
        await pending.Page.GotoAsync(CarePages.LinenStocktake);
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign stocktake return", Exact = true }).ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signed return", Exact = true })).ToHaveTextAsync("Signed tally awaiting count");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1626_0)]
    public async Task a_standard_return_opens_at_the_recorded_fractional_amount_or_blank()
    {
        var counted = await NewWorld();
        await counted.Page.GotoAsync(CarePages.LinenRecordedWeight);
        await Expect(counted.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("12.50");

        var newReturn = await NewWorld();
        await newReturn.Page.GotoAsync(CarePages.LinenUnrecordedWeight);
        await Expect(newReturn.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("");
    }

    private static ILocator PrintedSlip(IPage page)
    {
        var slip = page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Weighing slip", Exact = true });
        return slip.GetByRole(AriaRole.Spinbutton)
            .Or(slip.GetByText("Ward scale", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Receiving scale", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Kilograms", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Pounds", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Ward laundry", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Supplier delivery", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Net kilograms", new LocatorGetByTextOptions { Exact = true }))
            .Or(slip.GetByText("Gross pounds", new LocatorGetByTextOptions { Exact = true }));
    }
}
