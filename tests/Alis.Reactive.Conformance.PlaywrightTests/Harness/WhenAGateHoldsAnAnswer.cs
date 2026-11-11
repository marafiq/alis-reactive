using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAGateHoldsAnAnswer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAGateHoldsAnAnswer_the_held_answer_reads_the_word_seeded_while_it_was_held)]
    public async Task the_held_answer_reads_the_word_seeded_while_it_was_held()
    {
        var world = await NewWorld();
        await world.Seed(Seed.TheWord("North"));
        await world.Page.GotoAsync(WorldPages.Word);
        var armed = await world.Arm(CompositionGate.Answer);

        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read the word" }).ClickAsync();
        var held = await armed.Held();
        await world.Seed(Seed.TheWord("South"));
        await held.Release();

        await Expect(world.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("South");

        var ungated = await NewWorld();
        await ungated.Seed(Seed.TheWord("North"));
        await ungated.Page.GotoAsync(WorldPages.Word);
        await ungated.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read the word" }).ClickAsync();

        await Expect(ungated.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("North");
    }
}
