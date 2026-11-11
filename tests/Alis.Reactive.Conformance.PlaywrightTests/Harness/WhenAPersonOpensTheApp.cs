using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPersonOpensTheApp(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAPersonOpensTheApp_the_first_page_opens_in_a_world_of_its_own)]
    public async Task the_first_page_opens_in_a_world_of_its_own()
    {
        var seeded = await NewWorld();
        await seeded.Seed(Seed.Resident("Ada"));
        var person = await NewPerson();

        await person.GotoAsync(WorldPages.Records);
        await seeded.Page.GotoAsync(WorldPages.Records);

        await Expect(person.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Level = 1 })).ToHaveTextAsync("Residents");
        await Expect(person.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await Expect(seeded.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Ada"]);
    }
}
