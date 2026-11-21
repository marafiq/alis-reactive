using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonBodySendsAValueItsConverterWritesAsAList(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_32_0)]
    public async Task the_list_the_server_wrote_is_sent_and_a_read_that_reads_nothing_leaves_it_out()
    {
        var placed = await NewWorld();
        await placed.Page.GotoAsync(CarePages.BedSpotMoveOfAPlacedBed);
        var moved = await placed.Page.RunAndWaitForRequestAsync(() => placed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Move the bed", Exact = true }).ClickAsync(), "**/bed-spot-move/moved");
        Assert.That(moved.Method, Is.EqualTo("POST"));
        Assert.That(moved.PostData, Is.EqualTo("{\"spot\":[3,4]}"));
        await Expect(placed.Page.Locator("#moved")).ToHaveTextAsync("Moved to row 3 column 4");

        var unplaced = await NewWorld();
        await unplaced.Page.GotoAsync(CarePages.BedSpotMoveWithNoBed);
        var kept = await unplaced.Page.RunAndWaitForRequestAsync(() => unplaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Move the bed", Exact = true }).ClickAsync(), "**/bed-spot-move/moved");
        Assert.That(kept.PostData, Is.EqualTo("{}"));
        await Expect(unplaced.Page.Locator("#moved")).ToHaveTextAsync("Moved to row 0 column 0");
    }
}
