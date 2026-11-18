using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorChecksAResidentsDetails(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X20_0)]
    public async Task a_verified_identity_names_who_checked_it_and_its_receipt_opens_only_where_the_server_asks()
    {
        var verified = await NewWorld();
        await verified.Page.GotoAsync(CarePages.MoveIn);
        await Expect(verified.Page.Locator("#identity-review")).ToHaveTextAsync("none");
        await verified.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(verified.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        Assert.That(verified.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
        await verified.Page.GotoAsync(CarePages.MoveIn);
        await Expect(verified.Page.Locator("#identity-review")).ToHaveTextAsync("ID100, checked by Maya Chen");

        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.MoveIn);
        await moved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Move-in date", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await moved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Move-in date", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await moved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(moved.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        await Expect(moved.Page.Locator("#receipt-ID100")).ToHaveTextAsync("Identity review ID100: Helen Ward, born 1942-05-14, moving in 2026-11-25, 18 Oak Street.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X21_0)]
    public async Task an_email_confirmation_that_differs_is_refused_in_the_browser_and_no_review_leaves()
    {
        var mismatched = await NewWorld();
        await mismatched.Page.GotoAsync(CarePages.MoveIn);
        await mismatched.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await mismatched.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await mismatched.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(mismatched.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await mismatched.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Email confirmation", Exact = true }).FillAsync("helen@example.org");
        await mismatched.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true }).ClickAsync();
        await Expect(mismatched.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Email confirmation", Exact = true })
            .GetByText("The email addresses must match.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(mismatched.Page.Locator("#move-in_summary").GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The email addresses must match." });
        await Expect(mismatched.Page.Locator("#resident-status")).ToHaveTextAsync("");
        await mismatched.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(mismatched.Page.Locator("#move-in_summary").GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The email addresses must match." });
        Assert.That(mismatched.SentTo(CareEndpoints.MoveInIdentityReview), Is.EqualTo(0));
        await mismatched.Page.GotoAsync(CarePages.MoveIn);
        await Expect(mismatched.Page.Locator("#identity-review")).ToHaveTextAsync("none");

        var matching = await NewWorld();
        await matching.Page.GotoAsync(CarePages.MoveIn);
        await matching.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await matching.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await matching.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(matching.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await matching.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true }).ClickAsync();
        await Expect(matching.Page.Locator("#resident-status")).ToHaveTextAsync("Resident details ready");
        await matching.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(matching.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        Assert.That(matching.SentTo(CareEndpoints.MoveInIdentityReview), Is.EqualTo(1));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X22_0)]
    public async Task a_resident_who_already_holds_an_admission_is_refused_by_the_server_with_its_reason()
    {
        var admitted = await NewWorld();
        await admitted.Page.GotoAsync(CarePages.MoveIn);
        await Expect(admitted.Page.Locator("#active-admission")).ToHaveTextAsync("none");
        await admitted.Seed(MoveInChanges.TheResidentHoldsAnAdmission("R101"));
        await admitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(admitted.Page.Locator("#identity-status")).ToHaveTextAsync("Helen Ward already holds admission ADM101.");
        await admitted.Page.GotoAsync(CarePages.MoveIn);
        await Expect(admitted.Page.Locator("#active-admission")).ToHaveTextAsync("ADM101");
        await Expect(admitted.Page.Locator("#identity-review")).ToHaveTextAsync("none");

        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.MoveIn);
        await free.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(free.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        await free.Page.GotoAsync(CarePages.MoveIn);
        await Expect(free.Page.Locator("#identity-review")).ToHaveTextAsync("ID100, checked by Maya Chen");
        await Expect(free.Page.Locator("#active-admission")).ToHaveTextAsync("none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X23_0)]
    public async Task an_identity_review_whose_connection_drops_says_so_once_and_leaves_no_review()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInIdentityReview));
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.Locator("#identity-status")).ToHaveTextAsync("");
        Assert.That(dropped.SentTo(CareEndpoints.MoveInIdentityReview), Is.EqualTo(1));
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.Locator("#identity-review")).ToHaveTextAsync("none");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }
}
