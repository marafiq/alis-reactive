using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARulesConditionReadsABoxTheFormSends(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_condition)]
    public async Task the_message_shows_when_strict_is_ticked()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationCondition);
        await bench.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Strict", Exact = true }).CheckAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(bench.Page.Locator("#shown")).ToBeEmptyAsync();

        var unticked = await NewWorld();
        await unticked.Page.GotoAsync(BenchPages.ValidationCondition);
        await unticked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(unticked.Page.Locator("#shown")).ToHaveTextAsync("Saved");
    }
}
