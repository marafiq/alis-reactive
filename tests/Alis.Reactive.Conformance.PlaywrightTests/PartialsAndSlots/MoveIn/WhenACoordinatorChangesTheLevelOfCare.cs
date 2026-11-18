using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorChangesTheLevelOfCare(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X15_0)]
    public async Task a_confirmed_level_loads_its_own_questions_and_a_save_keeps_that_level_alone()
    {
        var confirmed = await NewWorld();
        await confirmed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(confirmed.Page.Locator("#saved-care")).ToHaveTextAsync("Assisted, assessment AS-100 on 2026-11-17, 2.50 hours, Uses a walking frame");
        await confirmed.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await confirmed.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(confirmed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeVisibleAsync();
        await Expect(confirmed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assistance hours", Exact = true })).ToHaveCountAsync(0);
        await confirmed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessment code", Exact = true }).FillAsync("MC-7");
        await confirmed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true }).CheckAsync();
        await confirmed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Representative contact", Exact = true }).FillAsync("CT1");
        await confirmed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(confirmed.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await confirmed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(confirmed.Page.Locator("#saved-care")).ToHaveTextAsync("Memory, assessment MC-7, secure unit consent given, representative CT1");
        await Expect(confirmed.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true })).ToBeCheckedAsync();
        await Expect(confirmed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeCheckedAsync();
        await Expect(confirmed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assistance hours", Exact = true })).ToHaveCountAsync(0);

        var skilled = await NewWorld();
        await skilled.Page.GotoAsync(CarePages.MoveIn);
        await skilled.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Skilled", Exact = true }).ClickAsync();
        await skilled.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(skilled.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Order reference", Exact = true })).ToBeVisibleAsync();
        await Expect(skilled.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X16_0)]
    public async Task a_declined_change_keeps_the_answers_on_the_page_and_checks_their_level_again()
    {
        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.MoveIn);
        await kept.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Assisted", Exact = true })).ToBeCheckedAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessment code", Exact = true })).ToHaveValueAsync("AS-100");
        await Expect(kept.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await kept.Page.GotoAsync(CarePages.MoveIn);
        await Expect(kept.Page.Locator("#saved-care")).ToHaveTextAsync("Assisted, assessment AS-100 on 2026-11-17, 2.50 hours, Uses a walking frame");
        await Expect(kept.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await replaced.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await replaced.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeVisibleAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true })).ToBeCheckedAsync();
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X17_0)]
    public async Task a_level_the_server_refuses_says_why_and_leaves_the_saved_care_as_it_was()
    {
        var closed = await NewWorld();
        await closed.Seed(MoveInChanges.TheBuildingIsClosed("B1"));
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await closed.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#care-status")).ToHaveTextAsync("Memory care admission is closed.");
        await Expect(closed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(closed.Page.Locator("#saved-care")).ToHaveTextAsync("Assisted, assessment AS-100 on 2026-11-17, 2.50 hours, Uses a walking frame");
        await Expect(closed.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Assisted", Exact = true })).ToBeCheckedAsync();

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.MoveIn);
        await open.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeVisibleAsync();
        await Expect(open.Page.Locator("#care-status")).ToHaveTextAsync("");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X18_0)]
    public async Task questions_whose_connection_drops_say_so_and_the_saved_answers_come_back_at_the_next_read()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInCareSections));
        await dropped.InstallTheClock();
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await dropped.HoldTheClock();
        await dropped.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await dropped.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await dropped.RunTheClockUntilItSends(CareEndpoints.MoveInCareSections, 4);
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await Expect(dropped.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assistance hours", Exact = true })).ToHaveCountAsync(0);
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.Locator("#saved-care")).ToHaveTextAsync("Assisted, assessment AS-100 on 2026-11-17, 2.50 hours, Uses a walking frame");
        await Expect(dropped.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Assessment code", Exact = true })).ToHaveValueAsync("AS-100");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await answered.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeVisibleAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X19_0)]
    public async Task questions_a_newer_level_replaced_never_join_the_page_and_a_save_holds_the_newer_level_alone()
    {
        var replaced = await NewWorld();
        var armed = await replaced.Arm(LanesGate.Respond, CareEndpoints.MoveInCareSections);
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await replaced.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await replaced.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        var memory = await armed.Held();
        await Expect(replaced.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true })).ToBeCheckedAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await replaced.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Skilled", Exact = true }).ClickAsync();
        await replaced.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Order reference", Exact = true })).ToBeVisibleAsync();
        await memory.Release();
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(replaced.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Order reference", Exact = true })).ToBeVisibleAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await replaced.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Order reference", Exact = true }).FillAsync("ORD-7788");
        await replaced.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Prescriber", Exact = true }).FillAsync("Dr Lee");
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.Locator("#saved-care")).ToHaveTextAsync("Skilled, order ORD-7788 on 2026-11-18, prescriber Dr Lee");

        var alone = await NewWorld();
        var only = await alone.Arm(LanesGate.Respond, CareEndpoints.MoveInCareSections);
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await alone.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Memory", Exact = true }).ClickAsync();
        await alone.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        var held = await only.Held();
        await Expect(alone.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToHaveCountAsync(0);
        await held.Release();
        await Expect(alone.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Secure unit consent", Exact = true })).ToBeVisibleAsync();
    }
}
