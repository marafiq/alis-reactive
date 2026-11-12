using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnElseWritesThePayloadBesideABranchThatAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_12_0)]
    public async Task the_else_writes_in_the_panels_turn_and_a_write_after_the_branches_is_refused_where_the_view_renders()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.FlaggedAlarm);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise unflagged", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise flagged", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 2");
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: none");
        await record.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Late write", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: "
                + "write it before the first request, Confirm or async method.",
            Exact = true,
        })).ToBeVisibleAsync();

        var writtenFirst = await NewWorld();
        await writtenFirst.Page.GotoAsync(CarePages.FlaggedAlarmWrittenFirst);
        await writtenFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise unflagged", Exact = true }).ClickAsync();
        await Expect(writtenFirst.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
    }
}
