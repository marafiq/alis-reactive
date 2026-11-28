using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Plugins;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageProvidesItsOwnPlugins(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M13_F2.X17_24_0)]
    public async Task a_plugin_is_read_written_called_and_heard_by_the_listening_its_declaration_names()
    {
        var shift = await NewWorld();
        await shift.Page.GotoAsync(BenchPages.PluginsShiftLog);
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a note", Exact = true }).ClickAsync();
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the log", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#log-state")).ToHaveTextAsync("quiet");
        await Expect(shift.Page.Locator("#entries")).ToHaveTextAsync("1");
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the entries to 5", Exact = true }).ClickAsync();
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a note", Exact = true }).ClickAsync();
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the log", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#log-state")).ToHaveTextAsync("busy");
        await Expect(shift.Page.Locator("#entries")).ToHaveTextAsync("7");
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Answer the items", Exact = true }).ClickAsync();
        await shift.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ada: 7", "Bea: 7" });
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the initials", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#initials-read")).ToHaveTextAsync("AL");
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign the log", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#log-signed")).ToHaveTextAsync("signed");
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Strike midnight", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#clock-struck")).ToHaveTextAsync("midnight");
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Free bed 4", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#bed-freed")).ToHaveTextAsync("4");

        await shift.Page.ReloadAsync();
        await shift.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the log", Exact = true }).ClickAsync();
        await Expect(shift.Page.Locator("#log-state")).ToHaveTextAsync("quiet");
        await Expect(shift.Page.Locator("#entries")).ToHaveTextAsync("");
    }
}
