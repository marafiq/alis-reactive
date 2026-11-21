using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoHandlersAskOnOneClick(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.M25_X5_0)]
    public async Task the_questions_take_their_turn_and_each_handler_reads_its_own_answer()
    {
        var yesThenNo = await NewWorld();
        await yesThenNo.Page.GotoAsync(CarePages.FamilyNotice);
        await yesThenNo.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = yesThenNo.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(asking.GetByText("Notify the family?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(yesThenNo.Page.Locator("#discharge")).ToHaveTextAsync("true");
        await Expect(yesThenNo.Page.Locator("#notify")).ToHaveTextAsync("false");

        var noThenYes = await NewWorld();
        await noThenYes.Page.GotoAsync(CarePages.FamilyNotice);
        await noThenYes.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = noThenYes.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asked.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(asked.GetByText("Notify the family?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(noThenYes.Page.Locator("#discharge")).ToHaveTextAsync("false");
        await Expect(noThenYes.Page.Locator("#notify")).ToHaveTextAsync("true");
    }
}
