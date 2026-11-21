using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnActionWaitsAtTheGate(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAnActionWaitsAtTheGate_the_actions_held_answer_reads_the_word_seeded_while_it_was_held)]
    public async Task the_actions_held_answer_reads_the_word_seeded_while_it_was_held()
    {
        var world = await NewWorld();
        await world.Seed(Seed.TheWord("North"));
        await world.Page.GotoAsync(WorldPages.WordDesk);
        var armed = await world.Arm(CompositionGate.Answer, WorldEndpoints.WordDeskAnswer);

        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ask the desk" }).ClickAsync();
        var held = await armed.Held();
        await world.Seed(Seed.TheWord("South"));
        await held.Release();

        await Expect(world.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("South");

        var ungated = await NewWorld();
        await ungated.Seed(Seed.TheWord("North"));
        await ungated.Page.GotoAsync(WorldPages.WordDesk);
        await ungated.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ask the desk" }).ClickAsync();

        await Expect(ungated.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("North");
    }
}
