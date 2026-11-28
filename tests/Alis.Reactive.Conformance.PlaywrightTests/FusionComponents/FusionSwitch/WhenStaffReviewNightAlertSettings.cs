using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffReviewNightAlertSettings(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F46.M21_X1135_0)]
    public async Task the_decision_review_reports_the_switchs_live_approval()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.AlertPreferencesWardManaged);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Decision review", Exact = true })).ToHaveTextAsync("Ward approval required");

        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.AlertPreferences);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Decision review", Exact = true })).ToHaveTextAsync("Resident can apply");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1136_0)]
    public async Task the_replacement_review_reports_the_previous_alert_state()
    {
        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferencesReplacementPaused);
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep alerts enabled", Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Policy status", Exact = true })).ToHaveTextAsync("Replacing paused setting");

        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesReplacement);
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep alerts enabled", Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Policy status", Exact = true })).ToHaveTextAsync("Replacing enabled setting");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1137_0)]
    public async Task routing_review_distinguishes_who_requested_a_change_before_it_applies()
    {
        var control = await NewWorld();
        await control.Page.GotoAsync(CarePages.AlertPreferences);
        await control.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Activate alert control", Exact = true }).ClickAsync();
        await Expect(control.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Request source", Exact = true })).ToHaveTextAsync("Control requested routing");

        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.AlertPreferences);
        await resident.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Request source", Exact = true })).ToHaveTextAsync("Resident requested routing");

        var panel = await NewWorld();
        await panel.Page.GotoAsync(CarePages.AlertPreferences);
        await panel.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(panel.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Request source", Exact = true })).ToHaveTextAsync("Panel requested routing");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1138_0)]
    public async Task saving_alerts_carries_the_switchs_current_preference()
    {
        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferences);
        await enabled.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save alert preferences", Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved preference", Exact = true })).ToHaveTextAsync("Night alerts saved as enabled");
        await enabled.Page.ReloadAsync();
        await Expect(AlertSwitch(enabled.Page)).ToBeCheckedAsync();

        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await paused.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save alert preferences", Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved preference", Exact = true })).ToHaveTextAsync("Night alerts saved as paused");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1139_0)]
    public async Task assigning_alerts_does_not_record_a_resident_activation()
    {
        var assigned = await NewWorld();
        await assigned.Page.GotoAsync(CarePages.AlertPreferences);
        await assigned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enable night alerts", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(assigned.Page)).ToBeCheckedAsync();
        await Expect(assigned.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Change receipt", Exact = true })).ToHaveTextAsync("No changes yet");

        var activated = await NewWorld();
        await activated.Page.GotoAsync(CarePages.AlertPreferences);
        await activated.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(AlertSwitch(activated.Page)).ToBeCheckedAsync();
        await Expect(activated.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Change receipt", Exact = true })).ToHaveTextAsync("Night alerts enabled");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1140_0)]
    public async Task the_access_review_reflects_the_current_editing_lock()
    {
        var locked = await NewWorld();
        await locked.Page.GotoAsync(CarePages.AlertPreferences);
        await locked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock and apply", Exact = true }).ClickAsync();
        await locked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review only", Exact = true }).ClickAsync();
        await Expect(locked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access review", Exact = true })).ToHaveTextAsync("Locked for review");

        var editable = await NewWorld();
        await editable.Page.GotoAsync(CarePages.AlertPreferencesLocked);
        await editable.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Unlock and apply", Exact = true }).ClickAsync();
        await editable.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review only", Exact = true }).ClickAsync();
        await Expect(editable.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access review", Exact = true })).ToHaveTextAsync("Editable");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1141_0)]
    public async Task the_direction_review_reflects_the_active_layout()
    {
        var rtl = await NewWorld();
        await rtl.Page.GotoAsync(CarePages.AlertPreferences);
        await rtl.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use right-to-left layout", Exact = true }).ClickAsync();
        await rtl.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review active layout", Exact = true }).ClickAsync();
        await Expect(rtl.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Direction review", Exact = true })).ToHaveTextAsync("Right to left");

        var ltr = await NewWorld();
        await ltr.Page.GotoAsync(CarePages.AlertPreferencesRightToLeft);
        await ltr.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use left-to-right layout", Exact = true }).ClickAsync();
        await ltr.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review active layout", Exact = true }).ClickAsync();
        await Expect(ltr.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Direction review", Exact = true })).ToHaveTextAsync("Left to right");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1142_0)]
    public async Task the_density_review_reports_the_active_display_setting()
    {
        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.AlertPreferences);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact display", Exact = true }).ClickAsync();
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review display density", Exact = true }).ClickAsync();
        await Expect(compact.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Density review", Exact = true })).ToHaveTextAsync("Compact switch layout");

        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.AlertPreferencesCompact);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use wide display", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review display density", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Density review", Exact = true })).ToHaveTextAsync("Wide switch layout");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1143_0)]
    public async Task the_wording_review_reports_the_active_label_setting()
    {
        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review active wording", Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording review", Exact = true })).ToHaveTextAsync("Night dispatch on");

        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferences);
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review active wording", Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording review", Exact = true })).ToHaveTextAsync("Night dispatch paused");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1144_0)]
    public async Task clearing_a_wording_override_preserves_the_rendered_label()
    {
        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear display wording", Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByText("Night dispatch on", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-switch-on");
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording review", Exact = true })).ToHaveTextAsync("No wording override");

        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferences);
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear display wording", Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByText("Night dispatch paused", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-switch-off");
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording review", Exact = true })).ToHaveTextAsync("No wording override");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1145_0)]
    public async Task the_destination_review_reports_the_selected_desk()
    {
        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.AlertDestination);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night destination", Exact = true }).ClickAsync();
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review alert destination", Exact = true }).ClickAsync();
        await Expect(night.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Destination review", Exact = true })).ToHaveTextAsync("Night desk");

        var day = await NewWorld();
        await day.Page.GotoAsync(CarePages.AlertDestination);
        await day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review alert destination", Exact = true }).ClickAsync();
        await Expect(day.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Destination review", Exact = true })).ToHaveTextAsync("Day desk");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1153_0)]
    public async Task the_routing_review_classifies_the_switchs_activation_and_pause()
    {
        var activated = await NewWorld();
        await activated.Page.GotoAsync(CarePages.AlertPreferences);
        await activated.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(activated.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activity receipt", Exact = true })).ToHaveTextAsync("Night alert activation recorded");

        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await paused.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activity receipt", Exact = true })).ToHaveTextAsync("Night alert pause recorded");
    }

    private static ILocator AlertSwitch(IPage page) => page.GetByRole(AriaRole.Switch, new PageGetByRoleOptions { Name = "Night alerts", Exact = true });
}
