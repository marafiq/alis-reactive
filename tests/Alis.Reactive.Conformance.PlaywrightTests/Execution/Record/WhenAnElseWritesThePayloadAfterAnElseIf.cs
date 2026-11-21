using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnElseWritesThePayloadAfterAnElseIf(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_13_0)]
    public async Task the_else_writes_in_the_panels_turn_and_is_refused_where_the_view_renders_after_a_condition_that_awaits()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.CountedAlarm);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise unflagged", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await record.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Write after an awaited check", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: "
                + "write it before the first request, Confirm or async method.",
            Exact = true,
        })).ToBeVisibleAsync();

        var flagChecked = await NewWorld();
        await flagChecked.Page.GotoAsync(CarePages.CountedAlarmWrittenAfterAFlagCheck);
        await flagChecked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise unflagged", Exact = true }).ClickAsync();
        await Expect(flagChecked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
    }
}
