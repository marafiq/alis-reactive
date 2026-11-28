using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffOpenLinenStations(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1633_0)]
    public async Task missing_and_zero_stock_have_distinct_recount_and_review_handoffs()
    {
        var outstanding = await NewWorld();
        await outstanding.Page.GotoAsync(CarePages.LinenStocktake);
        await outstanding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await outstanding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Begin recount", Exact = true }).ClickAsync();
        await Expect(outstanding.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Count towels before signing");
        await outstanding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send tally for review", Exact = true }).ClickAsync();
        await Expect(outstanding.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Return held pending a count");

        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.LinenStocktake);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Begin recount", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Confirm the empty trolley");
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send tally for review", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tally handoff", Exact = true })).ToHaveTextAsync("Empty cart ready to sign");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1634_0)]
    public async Task the_transcription_guide_and_signed_invoice_read_the_charge_in_their_respective_states()
    {
        var editing = await NewWorld();
        await editing.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await editing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read amount for transcription", Exact = true }).ClickAsync();
        await Expect(editing.Page.GetByRole(AriaRole.Spinbutton)).ToBeFocusedAsync();
        await Expect(editing.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transcription guide", Exact = true })).ToHaveTextAsync("12.5");

        var signed = await NewWorld();
        await signed.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await signed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read signed invoice", Exact = true }).ClickAsync();
        await Expect(signed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Invoice preview", Exact = true })).ToHaveTextAsync("$12.50");
        await Expect(signed.Page.GetByRole(AriaRole.Spinbutton)).Not.ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1635_0)]
    public async Task staffed_and_archived_stations_open_with_their_configured_counting_profiles()
    {
        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.LinenStaffedStation);
        await staff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review station profile", Exact = true }).ClickAsync();
        await staff.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await Expect(staff.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("10");
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Station profile", Exact = true })).ToHaveTextAsync("Staffed trolley counting");

        var archive = await NewWorld();
        await archive.Page.GotoAsync(CarePages.LinenArchivedStation);
        await archive.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review station profile", Exact = true }).ClickAsync();
        await Expect(archive.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("14.0");
        await Expect(archive.Page.GetByRole(AriaRole.Spinbutton)).ToHaveAttributeAsync("disabled", "");
        await Expect(archive.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Station profile", Exact = true })).ToHaveTextAsync("Archived supplier count");
    }
}
