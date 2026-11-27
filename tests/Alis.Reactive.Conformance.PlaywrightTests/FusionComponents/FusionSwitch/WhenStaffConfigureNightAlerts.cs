using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffConfigureNightAlerts(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F46.M21_X1120_0)]
    public async Task night_alerts_can_be_enabled_or_paused_from_the_panel()
    {
        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferences);
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enable night alerts", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(paused.Page)).ToBeCheckedAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Routing status", Exact = true })).ToHaveTextAsync("Send to night desk");

        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pause night alerts", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(enabled.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Routing status", Exact = true })).ToHaveTextAsync("Do not send alerts");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1121_0)]
    public async Task flipping_night_alerts_reports_the_committed_preference()
    {
        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferences);
        await paused.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Change receipt", Exact = true })).ToHaveTextAsync("Night alerts enabled");

        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await enabled.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Change receipt", Exact = true })).ToHaveTextAsync("Night alerts paused");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1122_0)]
    public async Task applying_the_panel_choice_reports_an_adjustment_without_native_activation()
    {
        var adjusted = await NewWorld();
        await adjusted.Page.GotoAsync(CarePages.AlertPreferences);
        await adjusted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(adjusted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activation receipt", Exact = true })).ToHaveTextAsync("Panel adjustment");
        await Expect(AlertSwitch(adjusted.Page)).ToBeCheckedAsync();

        var activated = await NewWorld();
        await activated.Page.GotoAsync(CarePages.AlertPreferences);
        await activated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Activate alert control", Exact = true }).ClickAsync();
        await Expect(activated.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activation receipt", Exact = true })).ToHaveTextAsync("Panel activated control");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1123_0)]
    public async Task activating_the_alert_control_from_the_panel_is_distinct_from_a_resident_click()
    {
        var panel = await NewWorld();
        await panel.Page.GotoAsync(CarePages.AlertPreferences);
        await panel.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Activate alert control", Exact = true }).ClickAsync();
        await Expect(panel.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activation receipt", Exact = true })).ToHaveTextAsync("Panel activated control");
        await Expect(AlertSwitch(panel.Page)).ToBeCheckedAsync();

        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.AlertPreferences);
        await resident.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Activation receipt", Exact = true })).ToHaveTextAsync("Resident activation");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1124_0)]
    public async Task a_ward_managed_preference_declines_the_same_choice_a_resident_can_apply()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.AlertPreferencesWardManaged);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Policy status", Exact = true })).ToHaveTextAsync("Managed by the ward");
        await Expect(AlertSwitch(ward.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });

        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.AlertPreferences);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(resident.Page)).ToBeCheckedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1125_0)]
    public async Task replacing_an_enabled_alert_setting_can_keep_the_alerts_enabled()
    {
        var replacement = await NewWorld();
        await replacement.Page.GotoAsync(CarePages.AlertPreferencesReplacement);
        await replacement.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep alerts enabled", Exact = true }).ClickAsync();
        await Expect(replacement.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Policy status", Exact = true })).ToHaveTextAsync("Replacing enabled setting");
        await Expect(AlertSwitch(replacement.Page)).ToBeCheckedAsync();

        var ordinary = await NewWorld();
        await ordinary.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await ordinary.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply panel choice", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(ordinary.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1126_0)]
    public async Task the_same_apply_respects_the_new_editing_lock_before_activating_the_switch()
    {
        var locked = await NewWorld();
        await locked.Page.GotoAsync(CarePages.AlertPreferences);
        await locked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock and apply", Exact = true }).ClickAsync();
        await Expect(locked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access review", Exact = true })).ToHaveTextAsync("Locked and paused");
        await Expect(AlertSwitch(locked.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(AlertSwitch(locked.Page)).ToHaveAttributeAsync("disabled", "");

        var unlocked = await NewWorld();
        await unlocked.Page.GotoAsync(CarePages.AlertPreferencesLocked);
        await unlocked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Unlock and apply", Exact = true }).ClickAsync();
        await Expect(unlocked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access review", Exact = true })).ToHaveTextAsync("Editable and enabled");
        await Expect(AlertSwitch(unlocked.Page)).ToBeCheckedAsync();
        await Expect(AlertSwitch(unlocked.Page)).Not.ToHaveAttributeAsync("disabled", "");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1127_0)]
    public async Task the_night_display_labels_follow_the_alert_preference()
    {
        var enabled = await NewWorld();
        await enabled.Page.GotoAsync(CarePages.AlertPreferencesEnabled);
        await enabled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await Expect(enabled.Page.GetByText("Night dispatch on", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-switch-on");

        var paused = await NewWorld();
        await paused.Page.GotoAsync(CarePages.AlertPreferences);
        await paused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night display", Exact = true }).ClickAsync();
        await Expect(paused.Page.GetByText("Night dispatch paused", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-switch-off");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1128_0)]
    public async Task switching_between_compact_and_wide_displays_replaces_the_control_style()
    {
        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.AlertPreferences);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact display", Exact = true }).ClickAsync();
        await Expect(compact.Page.GetByText("Compact display", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.AlertPreferencesCompact);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use wide display", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByText("Wide display", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1129_0)]
    public async Task the_alert_control_layout_changes_in_both_directions()
    {
        var rtl = await NewWorld();
        await rtl.Page.GotoAsync(CarePages.AlertPreferences);
        await rtl.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use right-to-left layout", Exact = true }).ClickAsync();
        await Expect(rtl.Page.GetByText("Right to left", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var ltr = await NewWorld();
        await ltr.Page.GotoAsync(CarePages.AlertPreferencesRightToLeft);
        await ltr.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use left-to-right layout", Exact = true }).ClickAsync();
        await Expect(ltr.Page.GetByText("Left to right", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1130_0)]
    public async Task the_focus_action_transfers_focus_to_the_alert_switch()
    {
        var control = await NewWorld();
        await control.Page.GotoAsync(CarePages.AlertPreferences);
        await control.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Focus alert control", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(control.Page)).ToBeFocusedAsync();

        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.AlertPreferences);
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review only", Exact = true }).ClickAsync();
        await Expect(review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review only", Exact = true })).ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1131_0)]
    public async Task closing_the_alert_preview_releases_the_switch_to_its_native_checkbox()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.AlertSwitchPreview);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close switch preview", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByText("Preview enabled", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(AlertSwitch(closed.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.AlertSwitchPreview);
        await Expect(open.Page.GetByText("Preview enabled", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(AlertSwitch(open.Page)).ToBeCheckedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1132_0)]
    public async Task the_native_destination_form_submits_the_desk_chosen_for_alerts()
    {
        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.AlertDestination);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use night destination", Exact = true }).ClickAsync();
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save alert destination", Exact = true }).ClickAsync();
        await Expect(night.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved destination", Exact = true })).ToHaveTextAsync("Night desk selected");

        var day = await NewWorld();
        await day.Page.GotoAsync(CarePages.AlertDestination);
        await day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save alert destination", Exact = true }).ClickAsync();
        await Expect(day.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Saved destination", Exact = true })).ToHaveTextAsync("Day desk selected");
    }

    private static ILocator AlertSwitch(IPage page) => page.GetByRole(AriaRole.Switch, new PageGetByRoleOptions { Name = "Night alerts", Exact = true });
}
