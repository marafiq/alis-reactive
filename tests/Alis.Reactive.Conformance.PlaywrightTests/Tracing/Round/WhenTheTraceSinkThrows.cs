using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.TracingSinkThrows))]
[Category(Hosts.TracingSinkThrowsName)]
public sealed class WhenTheTraceSinkThrows(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string Saves = Send + " > POST " + CareEndpoints.DoseIntakeSaves;

    [Test]
    [FailsFor(Promises.M26_F2.X20_5_0)]
    public async Task each_save_succeeds_and_the_console_shows_one_error()
    {
        var throwing = await NewWorld();
        await throwing.Page.GotoAsync(CarePages.DoseIntake);
        await throwing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(throwing.Page.Locator("#status")).ToHaveTextAsync("saved 1");
        await throwing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(throwing.Page.Locator("#status")).ToHaveTextAsync("saved 2");
        Assert.That(throwing.ConsoleErrors, Has.Count.EqualTo(1));

        var exported = await NewWorldUnder(Hosts.TracingName);
        await exported.Page.GotoAsync(CarePages.DoseIntake);
        await exported.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(exported.Page.Locator("#status")).ToHaveTextAsync("saved 1");
        await exported.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(exported.Page.Locator("#status")).ToHaveTextAsync("saved 2");
        Assert.That(exported.ConsoleErrors, Is.Empty);
        await exported.Page.GotoAsync(WorldPages.Collector);
        await Expect(exported.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Saves + " completed 200",
            Run + " completed",
            Send + " completed",
            Saves + " completed 200",
        ]);
    }
}
