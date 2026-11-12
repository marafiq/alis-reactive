using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Plugins;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoDeclarationsReachOnePlugin(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M13_F1.M13_X2_0)]
    public async Task the_plugin_is_heard_by_the_listening_of_the_declaration_that_declares_its_event()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(BenchPages.PluginsNightDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the hour", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#hour")).ToHaveTextAsync("12");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Strike midnight", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#clock-struck")).ToHaveTextAsync("midnight");

        var faceFirst = await NewWorld();
        await faceFirst.Page.GotoAsync(BenchPages.PluginsNightDeskFaceFirst);
        await faceFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the hour", Exact = true }).ClickAsync();
        await Expect(faceFirst.Page.Locator("#hour")).ToHaveTextAsync("12");
        await faceFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Strike midnight", Exact = true }).ClickAsync();
        await Expect(faceFirst.Page.Locator("#clock-struck")).ToHaveTextAsync("midnight");
    }
}
