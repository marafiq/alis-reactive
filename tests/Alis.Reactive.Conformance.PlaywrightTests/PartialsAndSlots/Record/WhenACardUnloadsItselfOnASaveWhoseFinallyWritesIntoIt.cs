using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardUnloadsItselfOnASaveWhoseFinallyWritesIntoIt(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_17_0)]
    public async Task the_finally_runs_before_the_release_the_card_empties_and_it_opens_again()
    {
        var stuck = await NewWorld();
        await stuck.Page.GotoAsync(CarePages.ShiftCardWithAStuckRadio);
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(stuck.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Handover card", Exact = true })).ToBeVisibleAsync();
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(stuck.Page.Locator("#status")).ToHaveTextAsync("1");
        await Expect(stuck.Page.Locator("#shift-card-slot")).ToBeEmptyAsync();
        await stuck.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(stuck.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Handover card", Exact = true })).ToBeVisibleAsync();

        var clean = await NewWorld();
        await clean.Page.GotoAsync(CarePages.ShiftCard);
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Handover card", Exact = true })).ToBeVisibleAsync();
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(clean.Page.Locator("#status")).ToHaveTextAsync("1");
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Handover card", Exact = true })).ToBeVisibleAsync();
        Assert.That(clean.ConsoleErrors, Is.Empty);
    }
}
