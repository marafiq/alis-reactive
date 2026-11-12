using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAGateIsHeldAtTwoEndpoints(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAGateIsHeldAtTwoEndpoints_each_endpoints_answer_waits_for_its_own_release)]
    public async Task each_endpoints_answer_waits_for_its_own_release()
    {
        var world = await NewWorld();
        await world.Seed(Seed.TheWord("North"));
        await world.Page.GotoAsync(WorldPages.Words);
        var armed = await world.Arm(CompositionGate.Answer, WorldEndpoints.WordAnswer);
        var armedAgain = await world.Arm(CompositionGate.Answer, WorldEndpoints.WordAnswerAgain);

        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read the word" }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read it again" }).ClickAsync();
        var held = await armed.Held();
        var heldAgain = await armedAgain.Held();
        await world.Seed(Seed.TheWord("South"));
        await heldAgain.Release();

        await Expect(world.Page.FrameLocator("#again").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("South");
        await Expect(world.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveCountAsync(0);

        await world.Seed(Seed.TheWord("East"));
        await held.Release();

        await Expect(world.Page.FrameLocator("#answer").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("East");

        var armedOnce = await NewWorld();
        await armedOnce.Seed(Seed.TheWord("North"));
        await armedOnce.Page.GotoAsync(WorldPages.Words);
        await armedOnce.Arm(CompositionGate.Answer, WorldEndpoints.WordAnswer);
        await armedOnce.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read it again" }).ClickAsync();

        await Expect(armedOnce.Page.FrameLocator("#again").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("North");
    }
}
