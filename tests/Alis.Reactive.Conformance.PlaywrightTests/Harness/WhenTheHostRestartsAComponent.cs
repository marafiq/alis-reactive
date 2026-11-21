using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheHostRestartsAComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenTheHostRestartsAComponent_the_element_holds_a_new_object_when_the_restart_returns)]
    public async Task the_element_holds_a_new_object_when_the_restart_returns()
    {
        var restarted = await NewWorld();
        await restarted.Page.GotoAsync(WorldPages.Host);
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 1" });
        await restarted.Restart("callbacks");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 2" });

        var kept = await NewWorld();
        await kept.Page.GotoAsync(WorldPages.Host);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 1" });
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 1", "own 1" });
    }
}
