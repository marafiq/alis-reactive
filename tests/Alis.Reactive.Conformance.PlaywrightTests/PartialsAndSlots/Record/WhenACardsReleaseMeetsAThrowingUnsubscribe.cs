using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardsReleaseMeetsAThrowingUnsubscribe(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_15_0)]
    public async Task every_owner_is_released_the_new_card_starts_its_finally_runs_and_the_step_stops_with_one_error()
    {
        var stuck = await NewWorld();
        await stuck.Page.GotoAsync(CarePages.RadioWardWithAStuckRadio);
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(stuck.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada Byron", Exact = true })).ToBeVisibleAsync();
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ben's card", Exact = true }).ClickAsync();
        await Expect(stuck.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ben Okafor", Exact = true })).ToBeVisibleAsync();
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the ward", Exact = true }).ClickAsync();
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(stuck.Page.Locator("#ben-tally")).ToHaveTextAsync("1");
        await Expect(stuck.Page.Locator("#finally")).ToHaveTextAsync("Ben settled");
        await Expect(stuck.Page.Locator("#card-heard")).ToHaveTextAsync("");
        await Expect(stuck.Page.Locator("#inner-heard")).ToHaveTextAsync("");
        await Expect(stuck.Page.Locator("#after")).ToHaveTextAsync("");
        Assert.That(stuck.ConsoleErrors, Has.Count.EqualTo(1));

        var clean = await NewWorld();
        await clean.Page.GotoAsync(CarePages.RadioWard);
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada Byron", Exact = true })).ToBeVisibleAsync();
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ben's card", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ben Okafor", Exact = true })).ToBeVisibleAsync();
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the ward", Exact = true }).ClickAsync();
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(clean.Page.Locator("#ben-tally")).ToHaveTextAsync("1");
        await Expect(clean.Page.Locator("#after")).ToHaveTextAsync("Ben opened");
        Assert.That(clean.ConsoleErrors, Is.Empty);
    }
}
