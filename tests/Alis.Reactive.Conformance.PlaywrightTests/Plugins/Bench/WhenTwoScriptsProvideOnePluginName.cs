using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Plugins;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoScriptsProvideOnePluginName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M13_F1.X17_26_0)]
    public async Task each_step_that_reaches_it_stops_and_the_rest_of_the_page_runs()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(BenchPages.PluginsNightWard);
        await Expect(ward.Page.Locator("#started")).ToHaveTextAsync("the page started");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the log", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#log-tried")).ToHaveTextAsync("tried");
        await Expect(ward.Page.Locator("#entries")).ToHaveTextAsync("");
        await Expect(ward.Page.Locator("#log-done")).ToHaveTextAsync("");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the lantern", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#lantern-tried")).ToHaveTextAsync("tried");
        await Expect(ward.Page.Locator("#colour")).ToHaveTextAsync("");
        await Expect(ward.Page.Locator("#lantern-done")).ToHaveTextAsync("");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start the round", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#round")).ToHaveTextAsync("started");
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the log", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#log-tried")).ToHaveTextAsync("tried");
        await Expect(ward.Page.Locator("#entries")).ToHaveTextAsync("");
        await Expect(ward.Page.Locator("#log-done")).ToHaveTextAsync("");

        var oneLog = await NewWorld();
        await oneLog.Page.GotoAsync(BenchPages.PluginsNightWardOneLog);
        await oneLog.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the log", Exact = true }).ClickAsync();
        await Expect(oneLog.Page.Locator("#entries")).ToHaveTextAsync("2");
        await Expect(oneLog.Page.Locator("#log-done")).ToHaveTextAsync("read");
        await oneLog.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the lantern", Exact = true }).ClickAsync();
        await Expect(oneLog.Page.Locator("#colour")).ToHaveTextAsync("amber");
        await Expect(oneLog.Page.Locator("#lantern-done")).ToHaveTextAsync("read");
    }
}
