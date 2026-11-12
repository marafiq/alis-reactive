using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Plugins;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScriptProvidesAPluginAfterBoot(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M13_F1.X17_25_0)]
    public async Task a_step_that_reaches_it_before_stops_its_own_pipeline_and_one_after_reaches_it()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(BenchPages.PluginsVisitorDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the visits", Exact = true }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Greet the visitor", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#greeting")).ToHaveTextAsync("welcome");
        await Expect(desk.Page.Locator("#count-tried")).ToHaveTextAsync("tried");
        await Expect(desk.Page.Locator("#visits")).ToHaveTextAsync("");
        await Expect(desk.Page.Locator("#count-done")).ToHaveTextAsync("");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the visitor book", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#book-state")).ToHaveTextAsync("book ready");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the visits", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#visits")).ToHaveTextAsync("3");
        await Expect(desk.Page.Locator("#count-done")).ToHaveTextAsync("counted");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign the book", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#book-own")).ToHaveTextAsync("signed");
        await Expect(desk.Page.Locator("#book-signed")).ToHaveTextAsync("");

        var open = await NewWorld();
        await open.Page.GotoAsync(BenchPages.PluginsVisitorDeskOpenAtLoad);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the visits", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#visits")).ToHaveTextAsync("3");
        await Expect(open.Page.Locator("#count-done")).ToHaveTextAsync("counted");
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign the book", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#book-signed")).ToHaveTextAsync("signed");
    }
}
