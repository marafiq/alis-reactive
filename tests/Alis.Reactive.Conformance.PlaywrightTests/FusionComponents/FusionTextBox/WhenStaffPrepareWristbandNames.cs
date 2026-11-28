using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffPrepareWristbandNames(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F49.M21_X1500_0)]
    public async Task staff_changes_and_directory_entries_request_the_right_wristband()
    {
        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.WristbandNames);
        await Entry(staff.Page).FillAsync("Nelly");
        await Entry(staff.Page).PressAsync("Tab");
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wristband decision", Exact = true })).ToHaveTextAsync("Staff amendment to Nell");

        var directory = await NewWorld();
        await directory.Page.GotoAsync(CarePages.WristbandNameBlank);
        await directory.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await Expect(directory.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wristband decision", Exact = true })).ToHaveTextAsync("Directory entry for Eleanor");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1501_0)]
    public async Task a_first_name_and_an_amended_name_offer_different_draft_actions()
    {
        var first = await NewWorld();
        await first.Page.GotoAsync(CarePages.WristbandNameBlank);
        await Entry(first.Page).FillAsync("Jo");
        await Expect(first.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wristband draft", Exact = true })).ToHaveTextAsync("Add a wristband name");
        await Expect(Entry(first.Page)).ToBeFocusedAsync();

        var replacement = await NewWorld();
        await replacement.Page.GotoAsync(CarePages.WristbandNames);
        await Entry(replacement.Page).FillAsync("Joy");
        await Expect(replacement.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wristband draft", Exact = true })).ToHaveTextAsync("Replace wristband name");
        await Expect(Entry(replacement.Page)).ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1502_0)]
    public async Task the_editor_announces_the_recorded_name_and_its_readiness_for_printing()
    {
        var recorded = await NewWorld();
        await recorded.Page.GotoAsync(CarePages.WristbandNames);
        await recorded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit wristband name", Exact = true }).ClickAsync();
        await Expect(Entry(recorded.Page)).ToBeFocusedAsync();
        await Expect(recorded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editing guidance", Exact = true })).ToHaveTextAsync("Editing the recorded name");
        await recorded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Finish editing", Exact = true }).ClickAsync();
        await Expect(recorded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print readiness", Exact = true })).ToHaveTextAsync("Ready to print");
        await Expect(Entry(recorded.Page)).Not.ToBeFocusedAsync();

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.WristbandNameBlank);
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit wristband name", Exact = true }).ClickAsync();
        await Expect(blank.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editing guidance", Exact = true })).ToHaveTextAsync("Adding a preferred name");
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Finish editing", Exact = true }).ClickAsync();
        await Expect(blank.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print readiness", Exact = true })).ToHaveTextAsync("Name still needed");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1503_0)]
    public async Task the_printing_lock_applies_before_the_desk_checks_the_field()
    {
        var printing = await NewWorld();
        await printing.Page.GotoAsync(CarePages.WristbandNames);
        await printing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock for printing", Exact = true }).ClickAsync();
        await Expect(printing.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editing access", Exact = true })).ToHaveTextAsync("Locked for printing");
        await Expect(Entry(printing.Page)).ToHaveAttributeAsync("disabled", "");

        var release = await NewWorld();
        await release.Page.GotoAsync(CarePages.WristbandNameLocked);
        await release.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release printing lock", Exact = true }).ClickAsync();
        await Expect(release.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editing access", Exact = true })).ToHaveTextAsync("Staff can edit");
        await Expect(Entry(release.Page)).ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1504_0)]
    public async Task protected_and_editable_names_show_their_own_entry_instructions()
    {
        var protect = await NewWorld();
        await protect.Page.GotoAsync(CarePages.WristbandNames);
        await protect.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Protect recorded name", Exact = true }).ClickAsync();
        await Expect(Entry(protect.Page)).ToHaveAttributeAsync("readonly", "");
        await Expect(Entry(protect.Page)).ToHaveAttributeAsync("placeholder", "Check against wristband");
        await Expect(protect.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Profile permission", Exact = true })).ToHaveTextAsync("Read-only profile");

        var release = await NewWorld();
        await release.Page.GotoAsync(CarePages.WristbandNameProtected);
        await release.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allow name edits", Exact = true }).ClickAsync();
        await Expect(Entry(release.Page)).ToHaveAttributeAsync("placeholder", "Enter preferred name");
        await Expect(release.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Profile permission", Exact = true })).ToHaveTextAsync("Editable profile");
        await Expect(Entry(release.Page)).Not.ToHaveAttributeAsync("readonly", "");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1505_0)]
    public async Task the_profile_reopens_with_the_registered_wristband_name()
    {
        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.WristbandNames);
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save preferred name", Exact = true }).ClickAsync();
        await Expect(named.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Profile receipt", Exact = true })).ToHaveTextAsync("Named profile saved");
        await named.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved profile", Exact = true }).ClickAsync();
        await Expect(Entry(named.Page)).ToHaveValueAsync("Eleanor");

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.WristbandNames);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove preferred name", Exact = true }).ClickAsync();
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save preferred name", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Profile receipt", Exact = true })).ToHaveTextAsync("Unnamed profile saved");
        await unnamed.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved profile", Exact = true }).ClickAsync();
        await Expect(Entry(unnamed.Page)).ToHaveValueAsync("");
        await Expect(unnamed.Page.GetByText("No wristband name on file", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1513_0)]
    public async Task only_a_desk_that_remembers_the_wristband_recovers_the_name_on_return()
    {
        var remember = await NewWorld();
        await remember.Page.GotoAsync(CarePages.WristbandNames);
        await remember.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember wristband here", Exact = true }).ClickAsync();
        await Expect(remember.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Draft storage", Exact = true })).ToHaveTextAsync("Remember on this desk");
        await remember.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await remember.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to the reception desk", Exact = true }).ClickAsync();
        await Expect(Entry(remember.Page)).ToHaveValueAsync("Eleanor");

        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.WristbandNames);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use resident file", Exact = true }).ClickAsync();
        await Expect(standard.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Draft storage", Exact = true })).ToHaveTextAsync("Use resident file");
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await standard.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to the reception desk", Exact = true }).ClickAsync();
        await Expect(Entry(standard.Page)).ToHaveValueAsync("Nell");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1514_0)]
    public async Task closing_the_preview_returns_the_original_field_while_rebuilding_keeps_the_live_name()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WristbandNames);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close wristband preview", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Preview state", Exact = true })).ToHaveTextAsync("Preview closed");
        await Expect(Entry(closed.Page)).ToHaveValueAsync("Nell");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.WristbandNames);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Rebuild wristband preview", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Preview state", Exact = true })).ToHaveTextAsync("Preview open");
        await Expect(Entry(open.Page)).ToHaveValueAsync("Eleanor");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1515_0)]
    public async Task the_recovery_copy_uses_the_name_in_the_live_wristband_snapshot()
    {
        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.WristbandNames);
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use directory name", Exact = true }).ClickAsync();
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive recovery copy", Exact = true }).ClickAsync();
        await Expect(named.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Recovery copy", Exact = true })).ToHaveTextAsync("Recovery copy for Eleanor");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.WristbandNameBlank);
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive recovery copy", Exact = true }).ClickAsync();
        await Expect(blank.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Recovery copy", Exact = true })).ToHaveTextAsync("No name in recovery copy");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1520_0)]
    public async Task a_directory_withdrawal_is_distinct_from_a_staff_member_clearing_the_printed_name()
    {
        var directory = await NewWorld();
        await directory.Page.GotoAsync(CarePages.WristbandNames);
        await directory.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove preferred name", Exact = true }).ClickAsync();
        await Expect(directory.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Name withdrawal", Exact = true })).ToHaveTextAsync("Directory name withdrawn");

        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.WristbandNames);
        await Entry(staff.Page).ClearAsync();
        await Entry(staff.Page).PressAsync("Tab");
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Name withdrawal", Exact = true })).ToHaveTextAsync("Staff cleared the printed name");
    }

    private static ILocator Entry(IPage page) => page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Wristband name", Exact = true }).GetByRole(AriaRole.Textbox);
}
