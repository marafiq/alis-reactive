using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAWorldOpensUnderAnotherHost(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAWorldOpensUnderAnotherHost_the_world_is_seeded_and_served_by_the_host_the_test_names)]
    public async Task the_world_is_seeded_and_served_by_the_host_the_test_names()
    {
        var numbersAsText = await NewWorldUnder(Hosts.NumbersAsTextName);
        await numbersAsText.Seed(Seed.TheWord("East"));
        await numbersAsText.Page.GotoAsync(WorldPages.WordSlip);

        await Expect(numbersAsText.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("{\"word\":\"East\",\"letters\":\"4\"}");

        var fixtures = await NewWorld();
        await fixtures.Seed(Seed.TheWord("North"));
        await fixtures.Page.GotoAsync(WorldPages.WordSlip);

        await Expect(fixtures.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("{\"word\":\"North\",\"letters\":5}");
    }
}
