using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoHandlersWriteThePayloadInThePanelsTurn(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.X11_19_0)]
    public async Task the_panel_shows_what_both_handlers_wrote_once_its_listeners_return()
    {
        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.AnsweredAlarm);
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Answer", Exact = true })).ToHaveTextAsync("One by Lambda");

        var unanswered = await NewWorld();
        await unanswered.Page.GotoAsync(CarePages.AnsweredAlarmWithOneHandler);
        await unanswered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(unanswered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await Expect(unanswered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Answer", Exact = true })).ToHaveTextAsync("One unanswered");
    }
}
