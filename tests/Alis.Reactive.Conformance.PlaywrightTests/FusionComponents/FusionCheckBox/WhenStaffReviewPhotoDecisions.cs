using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffReviewPhotoDecisions(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F10.M21_X1300_0)]
    public async Task the_badge_desk_distinguishes_pending_from_resolved_permission()
    {
        var pending = await NewWorld();
        await pending.Page.GotoAsync(CarePages.PhotoDecision);
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Defer the decision", Exact = true }).ClickAsync();
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview badge", Exact = true }).ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Badge instructions", Exact = true })).ToHaveTextAsync("Contact the resident before issuing a badge");
        await Expect(pending.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Indeterminate = true });

        var resolved = await NewWorld();
        await resolved.Page.GotoAsync(CarePages.PhotoDecisionPending);
        await resolved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Resolve without permission", Exact = true }).ClickAsync();
        await resolved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview badge", Exact = true }).ClickAsync();
        await Expect(resolved.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Badge instructions", Exact = true })).ToHaveTextAsync("Issue a badge without a photo");
        await Expect(resolved.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1301_0)]
    public async Task saving_the_signed_choice_records_the_correct_photo_decision()
    {
        var permitted = await NewWorld();
        await permitted.Page.GotoAsync(CarePages.PhotoDecision);
        await permitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply signed permission", Exact = true }).ClickAsync();
        await permitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save photo decision", Exact = true }).ClickAsync();
        await Expect(permitted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Photo permission recorded");
        await Expect(permitted.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync();

        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.PhotoDecisionGranted);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply signed refusal", Exact = true }).ClickAsync();
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save photo decision", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Photo refusal recorded");
        await Expect(refused.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1302_0)]
    public async Task the_consent_history_records_the_decision_and_who_activated_it()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.PhotoDecision);
        await resident.Page.GetByText("Photo permission", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Consent history", Exact = true })).ToHaveTextAsync("Resident granted photo permission");

        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.PhotoDecisionGranted);
        await staff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply delegated correction", Exact = true }).ClickAsync();
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Consent history", Exact = true })).ToHaveTextAsync("Staff recorded photo refusal");
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1303_0)]
    public async Task locking_or_reopening_a_signed_decision_controls_the_next_correction()
    {
        var locked = await NewWorld();
        await locked.Page.GotoAsync(CarePages.PhotoDecision);
        await locked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock and apply correction", Exact = true }).ClickAsync();
        await Expect(locked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Correction review", Exact = true })).ToHaveTextAsync("Signed decision locked");
        await Expect(locked.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });

        var reopened = await NewWorld();
        await reopened.Page.GotoAsync(CarePages.PhotoDecisionLocked);
        await reopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reopen and apply correction", Exact = true }).ClickAsync();
        await Expect(reopened.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Correction review", Exact = true })).ToHaveTextAsync("Correction applied");
        await Expect(reopened.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1304_0)]
    public async Task the_signature_review_returns_keyboard_focus_to_the_chosen_field()
    {
        var decision = await NewWorld();
        await decision.Page.GotoAsync(CarePages.PhotoDecision);
        await decision.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit photo decision", Exact = true }).ClickAsync();
        await Expect(decision.Page.GetByRole(AriaRole.Checkbox)).ToBeFocusedAsync();

        var signature = await NewWorld();
        await signature.Page.GotoAsync(CarePages.PhotoDecision);
        await signature.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit signature", Exact = true }).ClickAsync();
        await Expect(signature.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident signature", Exact = true })).ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1315_0)]
    public async Task confirming_a_pending_choice_with_space_retains_the_underlying_decision()
    {
        var granted = await NewWorld();
        await granted.Page.GotoAsync(CarePages.PhotoDecisionPendingGranted);
        await granted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit photo decision", Exact = true }).ClickAsync();
        await granted.Page.Keyboard.PressAsync("Space");
        await granted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview badge", Exact = true }).ClickAsync();
        await Expect(granted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Consent history", Exact = true })).ToHaveTextAsync("Resident granted photo permission");
        await Expect(granted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Badge instructions", Exact = true })).ToHaveTextAsync("Issue a badge with a photo");

        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.PhotoDecisionPending);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit photo decision", Exact = true }).ClickAsync();
        await refused.Page.Keyboard.PressAsync("Space");
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview badge", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Consent history", Exact = true })).ToHaveTextAsync("Resident declined photo permission");
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Badge instructions", Exact = true })).ToHaveTextAsync("Issue a badge without a photo");
    }
}
