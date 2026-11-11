using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoWorldsRun(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenTwoWorldsRun_each_world_lists_only_the_residents_it_seeded)]
    public async Task each_world_lists_only_the_residents_it_seeded()
    {
        var first = await NewWorld();
        var second = await NewWorld();
        await first.Seed(Seed.Resident("Ada"));
        await second.Seed(Seed.Resident("Ben"));

        await first.Page.GotoAsync(WorldPages.Records);
        await second.Page.GotoAsync(WorldPages.Records);

        await Expect(first.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Ada"]);
        await Expect(second.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Ben"]);

        await first.Seed(Seed.Resident("Ben"));
        await first.Page.ReloadAsync();

        await Expect(first.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Ada", "Ben"]);
    }
}
