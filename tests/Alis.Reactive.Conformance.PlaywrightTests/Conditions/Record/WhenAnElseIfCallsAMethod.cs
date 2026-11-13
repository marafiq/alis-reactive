using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnElseIfCallsAMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_14_0)]
    public async Task the_method_runs_only_when_no_branch_before_it_ran_and_its_branch_waits_for_its_answer()
    {
        var escalated = await NewWorld();
        await escalated.Seed(Seed.TheCheckEndpointsAnswer(true));
        await escalated.Page.GotoAsync(CarePages.PainEscalation);
        var pain = escalated.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Pain", Exact = true });
        var assess = escalated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assess", Exact = true });
        var nurseCalls = escalated.Page.Locator("#nurse").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Calls", Exact = true });
        var doctorCalls = escalated.Page.Locator("#doctor").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Calls", Exact = true });

        await pain.FillAsync("9");
        await assess.ClickAsync();
        await Expect(escalated.Page.Locator("#escalation")).ToHaveTextAsync("Ambulance");
        await Expect(nurseCalls).ToHaveTextAsync("0");
        await Expect(doctorCalls).ToHaveTextAsync("0");

        await pain.FillAsync("5");
        await escalated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Free", Exact = true }).ClickAsync();
        await assess.ClickAsync();
        await Expect(escalated.Page.Locator("#escalation")).ToHaveTextAsync("Nurse");
        await Expect(nurseCalls).ToHaveTextAsync("1");
        await Expect(doctorCalls).ToHaveTextAsync("0");

        var armed = await escalated.Arm(RunGate.Settle);
        await pain.FillAsync("5");
        await escalated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Busy", Exact = true }).ClickAsync();
        await assess.ClickAsync();
        var held = await armed.Held();
        await Expect(escalated.Page.Locator("#escalation")).ToHaveTextAsync("Nurse");
        await held.Release();
        await Expect(escalated.Page.Locator("#escalation")).ToHaveTextAsync("Doctor");
        await Expect(doctorCalls).ToHaveTextAsync("1");

        var declined = await NewWorld();
        await declined.Seed(Seed.TheCheckEndpointsAnswer(false));
        await declined.Page.GotoAsync(CarePages.PainEscalation);
        var armedAgain = await declined.Arm(RunGate.Settle);
        await declined.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Pain", Exact = true }).FillAsync("5");
        await declined.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Busy", Exact = true }).ClickAsync();
        await declined.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assess", Exact = true }).ClickAsync();
        var heldAgain = await armedAgain.Held();
        await heldAgain.Release();
        await Expect(declined.Page.Locator("#escalation")).ToHaveTextAsync("Wait");
    }
}
