using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorVerifiesThePayer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X120_0)]
    public async Task the_verified_policy_opens_its_monthly_coverage_and_records_its_reference()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#policy-details")).ToHaveTextAsync("Harbor Mutual covers up to 3500.00 per month.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("V100");
        await Expect(world.Page.Locator("#verification-context")).ToHaveTextAsync("Allocation reference DIN, community C1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Funding source", Exact = true }).SelectOptionAsync("Private");
        await Expect(world.Page.GetByText("Private funding", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Private funding. No secondary coverage.");
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");

        var optional = await NewWorld();
        await optional.Page.GotoAsync(CarePages.MoveIn);
        await optional.Page.Locator("#allocation-panel").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowUp");
        await optional.Page.Locator("#allocation-panel").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await optional.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await optional.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        await optional.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(optional.Page.Locator("#policy-details")).ToHaveTextAsync("Harbor Mutual covers up to 3500.00 per month.");
        await optional.Page.GotoAsync(CarePages.MoveIn);
        await Expect(optional.Page.Locator("#verification-context")).ToHaveTextAsync("No allocation reference supplied");

        var benefits = await NewWorld();
        await benefits.Page.GotoAsync(CarePages.MoveIn);
        await benefits.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Funding source", Exact = true }).SelectOptionAsync("Benefits");
        await Expect(benefits.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Primary policy number", Exact = true })).ToHaveValueAsync("POL-100");
        await benefits.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(benefits.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await benefits.Page.Locator("#allocation-panel").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowUp");
        await benefits.Page.Locator("#allocation-panel").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await benefits.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await benefits.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        await benefits.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(benefits.Page.Locator("#payer-verification_summary")).ToHaveTextAsync("Benefits coverage requires an allocation reference.");
        await benefits.Page.GotoAsync(CarePages.MoveIn);
        await Expect(benefits.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X121_0)]
    public async Task missing_consent_stays_in_the_drawer_and_no_verification_leaves()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#payer-verification_summary")).ToHaveTextAsync("Consent is required.");
        Assert.That(world.SentTo(CareEndpoints.MoveInPayerVerification), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X122_0)]
    public async Task an_inactive_policy_is_refused_without_recording_a_verification()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Policy to verify", Exact = true }).FillAsync("POL-200");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#verification-status")).ToHaveTextAsync("POL-200 is inactive.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X123_0)]
    public async Task a_broken_verification_connection_leaves_the_drawer_usable()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInPayerVerification));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#verification-status")).ToHaveTextAsync("Verification could not reach the payer.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X124_0)]
    public async Task the_pending_verification_keeps_its_drawer_open_while_the_main_draft_saves()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        var verification = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInPayerVerification);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        var verificationHeld = await verification.Held();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#verification-status")).ToHaveTextAsync("Wait for the payer to finish verification.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await verificationHeld.Release();
        await Expect(world.Page.Locator("#policy-details")).ToHaveTextAsync("Harbor Mutual covers up to 3500.00 per month.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("V100");

        var changed = await NewWorld();
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await changed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "I consent to verification", Exact = true }).CheckAsync();
        var beforeChange = await changed.Arm(CompositionGate.Answer, CareEndpoints.MoveInPayerVerification);
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        var oldPayer = await beforeChange.Held();
        await changed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Funding source", Exact = true }).SelectOptionAsync("Private");
        await Expect(changed.Page.GetByText("Private funding", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(changed.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await oldPayer.Release();
        await Expect(changed.Page.Locator("#verification-status")).ToHaveTextAsync("The saved payer changed. Reopen payer verification.");
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(changed.Page.Locator("#payer-on-file")).ToHaveTextAsync("Private funding. No secondary coverage.");
        await Expect(changed.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X24_0)]
    public async Task closing_invalid_independent_payer_details_keeps_resident_verification_clean()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer verification", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify policy", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#payer-verification_summary")).ToHaveTextAsync("Consent is required.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary").GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify identity", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#identity-status")).ToHaveTextAsync("Identity checked by Maya Chen");
        await Expect(world.Page.Locator("#move-in_summary").GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#identity-review")).ToHaveTextAsync("ID100, checked by Maya Chen");
        await Expect(world.Page.Locator("#verification-on-file")).ToHaveTextAsync("No verification");
    }
}
