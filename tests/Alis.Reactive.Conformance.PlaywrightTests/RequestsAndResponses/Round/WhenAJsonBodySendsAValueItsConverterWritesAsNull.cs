using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonBodySendsAValueItsConverterWritesAsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_34_0)]
    public async Task the_null_the_server_wrote_for_its_default_is_sent_and_a_read_that_reads_nothing_leaves_it_out()
    {
        var uncoded = await NewWorld();
        await uncoded.Page.GotoAsync(CarePages.RoomCodeChangeOfAnUncodedRoom);
        var changed = await uncoded.Page.RunAndWaitForRequestAsync(() => uncoded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Change the room", Exact = true }).ClickAsync(), "**/room-code-change/changed");
        Assert.That(changed.Method, Is.EqualTo("POST"));
        Assert.That(changed.PostData, Is.EqualTo("{\"code\":null}"));
        await Expect(uncoded.Page.Locator("#changed")).ToHaveTextAsync("Room code none");

        var roomless = await NewWorld();
        await roomless.Page.GotoAsync(CarePages.RoomCodeChangeWithNoRoom);
        var kept = await roomless.Page.RunAndWaitForRequestAsync(() => roomless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Change the room", Exact = true }).ClickAsync(), "**/room-code-change/changed");
        Assert.That(kept.PostData, Is.EqualTo("{}"));
        await Expect(roomless.Page.Locator("#changed")).ToHaveTextAsync("Room code A1");
    }
}
