using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardIsLoadedIntoASlotThreeTimes(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_2_0)]
    public async Task a_note_runs_the_open_cards_handler_once_and_no_cards_handler_once_the_card_is_closed()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.FallWatch);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada Byron", Exact = true })).ToBeVisibleAsync();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ben's card", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ben Okafor", Exact = true })).ToBeVisibleAsync();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada Byron", Exact = true })).ToBeVisibleAsync();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Note a fall", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#call-list")).ToHaveTextAsync("The ward heard the note Ada Byron heard the note");

        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(ward.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada Byron", Exact = true })).ToBeHiddenAsync();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Note a fall", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#call-list")).ToHaveTextAsync("The ward heard the note Ada Byron heard the note The ward heard the note");
    }
}
