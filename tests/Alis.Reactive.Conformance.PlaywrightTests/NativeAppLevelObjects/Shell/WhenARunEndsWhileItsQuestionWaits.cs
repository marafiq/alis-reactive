using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARunEndsWhileItsQuestionWaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.M25_X12_0)]
    public async Task the_waiting_question_of_an_ended_run_never_shows()
    {
        var refreshed = await NewWorld();
        await refreshed.Page.GotoAsync(CarePages.FamilyDischarge);
        var family = refreshed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Family", Exact = true });
        await Expect(family.GetByText("Family contact: Grace", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        var armed = await refreshed.Arm(CompositionGate.Answer, CareEndpoints.FamilyRefresh);
        await refreshed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh family", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await family.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = refreshed.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await held.Release();
        await Expect(family.GetByText("Family details refreshed.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(asking).ToBeHiddenAsync();

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.FamilyDischarge);
        var panel = kept.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Family", Exact = true });
        await panel.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = kept.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asked.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(asked.GetByText("Notify the family?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(panel.Locator("#notify")).ToHaveTextAsync("false");
    }
}
