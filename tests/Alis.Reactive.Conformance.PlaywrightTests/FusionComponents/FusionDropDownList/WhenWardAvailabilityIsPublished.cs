using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenWardAvailabilityIsPublished(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1824_0)]
    public async Task the_ward_lookup_shows_loading_until_the_on_call_reply_or_refusal()
    {
        var published = await NewWorld();
        await published.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        var next = await published.Arm(LanesGate.Respond, CareEndpoints.WardAvailability);
        await published.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check on-call wards", Exact = true }).ClickAsync();
        var waiting = await next.Held();
        await Expect(published.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Img)).ToBeVisibleAsync();
        await waiting.Release();
        await Expect(published.Page.Locator("#on-call-status")).ToHaveTextAsync("On-call wards updated.");
        await Expect(published.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Img)).ToBeHiddenAsync();
        await published.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(published.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak", "Willow", "Hazel" });

        var refused = await NewWorld();
        await refused.Seed(Seed.TheEndpointAnswers(CareEndpoints.WardAvailability, 500));
        await refused.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        var unavailable = await refused.Arm(LanesGate.Respond, CareEndpoints.WardAvailability);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check on-call wards", Exact = true }).ClickAsync();
        var held = await unavailable.Held();
        await Expect(refused.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Img)).ToBeVisibleAsync();
        await held.Release();
        await Expect(refused.Page.Locator("#on-call-status")).ToHaveTextAsync("The on-call directory is unavailable.");
        await Expect(refused.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Img)).ToBeHiddenAsync();
        await refused.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "On-call availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1825_0)]
    public async Task the_published_bulletin_changes_only_after_both_directory_checks()
    {
        var released = await NewWorld();
        await released.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await released.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(released.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await released.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await released.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Publish on-call bulletin", Exact = true }).ClickAsync();
        await Expect(released.Page.Locator("#bulletin-status")).ToHaveTextAsync("Three wards are on call.");
        await released.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(released.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak", "Willow", "Hazel" });

        var withheld = await NewWorld();
        await withheld.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await withheld.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(withheld.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await withheld.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await withheld.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Withhold publication", Exact = true }).CheckAsync();
        await withheld.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Publish on-call bulletin", Exact = true }).ClickAsync();
        await Expect(withheld.Page.Locator("#bulletin-status")).ToHaveTextAsync("Publication withheld.");
        await withheld.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(withheld.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });

        var pending = await NewWorld();
        await pending.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await pending.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await pending.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await pending.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Require final review", Exact = true }).CheckAsync();
        await pending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Publish on-call bulletin", Exact = true }).ClickAsync();
        await Expect(pending.Page.Locator("#bulletin-status")).ToHaveTextAsync("The completed bulletin requires review.");
        await pending.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Published bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(pending.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1826_0)]
    public async Task a_personal_workstation_remembers_its_ward_and_a_shared_one_uses_the_default()
    {
        var personal = await NewWorld();
        await personal.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await personal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember this workstation", Exact = true }).ClickAsync();
        await Expect(personal.Page.Locator("#station-status")).ToHaveTextAsync("Ward choices are remembered here.");
        await personal.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(personal.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await personal.Page.GotoAsync(CarePages.WardPlacement);
        await personal.Page.GotoAsync(CarePages.PersonalWardBulletin);
        await Expect(personal.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");

        var shared = await NewWorld();
        await shared.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use shared workstation", Exact = true }).ClickAsync();
        await Expect(shared.Page.Locator("#station-status")).ToHaveTextAsync("This workstation uses the default ward.");
        await shared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(shared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await shared.Page.GotoAsync(CarePages.WardPlacement);
        await shared.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await Expect(shared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Workstation choice", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1827_0)]
    public async Task the_accessible_and_compact_placement_cards_render_their_own_width_and_direction()
    {
        var accessible = await NewWorld();
        await accessible.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await accessible.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accessible placement card", Exact = true }).ClickAsync();
        await Expect(accessible.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).First).ToHaveAttributeAsync("style", "width: 420px;");
        await Expect(accessible.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).First).ToHaveClassAsync("e-input-group e-control-wrapper e-ddl e-lib e-keyboard e-valid-input e-rtl accessible-placement");
        await Expect(accessible.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await accessible.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print placement card", Exact = true }).ClickAsync();
        await Expect(accessible.Page.Locator("#printed-card-route")).ToHaveTextAsync("Attach the large-print right-to-left card to the resident copy.");

        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Compact placement card", Exact = true }).ClickAsync();
        await Expect(compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).First).ToHaveAttributeAsync("style", "width: 60%;");
        await Expect(compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).First).ToHaveClassAsync("e-input-group e-control-wrapper e-ddl e-lib e-keyboard e-valid-input compact-placement");
        await Expect(compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement card", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print placement card", Exact = true }).ClickAsync();
        await Expect(compact.Page.Locator("#printed-card-route")).ToHaveTextAsync("File the compact left-to-right slip with the receiving ward.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1828_0)]
    public async Task a_held_transfer_panel_stays_closed_while_an_available_panel_lists_wards()
    {
        var available = await NewWorld();
        await available.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await available.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open transfer panel", Exact = true }).ClickAsync();
        await Expect(available.Page.Locator("#panel-status")).ToHaveTextAsync("Choose the receiving ward.");
        await Expect(available.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });

        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await held.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Hold transfer panel", Exact = true }).CheckAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open transfer panel", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#panel-status")).ToHaveTextAsync("Obtain placement clearance first.");
        await Expect(held.Page.GetByRole(AriaRole.Listbox)).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1829_0)]
    public async Task only_provisional_placements_offer_a_clear_action()
    {
        var provisional = await NewWorld();
        await provisional.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await provisional.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allow provisional changes", Exact = true }).ClickAsync();
        await provisional.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Provisional destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(provisional.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Provisional destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await Expect(provisional.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Provisional destination", Exact = true }).GetByLabel("close")).ToBeVisibleAsync();
        await provisional.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Provisional destination", Exact = true }).GetByLabel("close").ClickAsync();
        await Expect(provisional.Page.Locator("#provisional-note")).ToHaveTextAsync("Choose a provisional destination.");
        await Expect(provisional.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Provisional destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("");

        var confirmed = await NewWorld();
        await confirmed.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await confirmed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Confirm destination", Exact = true }).ClickAsync();
        await confirmed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(confirmed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await Expect(confirmed.Page.Locator("#confirmed-note")).ToHaveTextAsync("Maple is confirmed.");
        await Expect(confirmed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByLabel("close")).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1830_0)]
    public async Task each_ward_supplies_its_own_clinical_badge_and_selected_caption()
    {
        var detailed = await NewWorld();
        await detailed.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await detailed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show clinical badges", Exact = true }).ClickAsync();
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(detailed.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Open care ward", "Secure care ward" });
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(detailed.Page.GetByText("Secure placement handover", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show short badges", Exact = true }).ClickAsync();
        await compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(compact.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Open", "Secure" });
        await compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await compact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward badges", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(compact.Page.GetByText("Open transfer", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1831_0)]
    public async Task numeric_and_boolean_keys_retrieve_their_own_full_referral_rows()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review observation referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#bed-contact")).ToHaveTextAsync("Maple observation bed");
        await Expect(desk.Page.Locator("#escort-contact")).ToHaveTextAsync("Maple independent bay");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review transfer referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#bed-contact")).ToHaveTextAsync("Cedar transfer bed");
        await Expect(desk.Page.Locator("#escort-contact")).ToHaveTextAsync("Cedar assisted bay");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review unmatched referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#bed-contact")).ToHaveTextAsync("Send this bed referral to review.");
        await Expect(desk.Page.Locator("#escort-contact")).ToHaveTextAsync("Send this assistance flag to review.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1832_0)]
    public async Task short_and_held_searches_retain_the_published_ward_directory()
    {
        var abbreviated = await NewWorld();
        await abbreviated.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await abbreviated.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(abbreviated.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await abbreviated.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("s");
        await Expect(abbreviated.Page.Locator("#service-instruction")).ToHaveTextAsync("Enter a full service code.");
        await Expect(abbreviated.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });

        var active = await NewWorld();
        await active.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await active.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(active.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await active.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("a");
        await Expect(active.Page.Locator("#service-instruction")).ToHaveTextAsync("Available on-call services.");
        await Expect(active.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak", "Willow", "Hazel" });

        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.WardAvailabilityBulletin);
        await held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Service bulletin", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await held.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("h");
        await Expect(held.Page.Locator("#service-instruction")).ToHaveTextAsync("This service is held for review.");
        await Expect(held.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
    }
}
