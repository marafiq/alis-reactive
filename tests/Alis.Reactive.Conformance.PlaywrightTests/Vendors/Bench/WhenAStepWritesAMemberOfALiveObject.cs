using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesAMemberOfALiveObject(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_object_memberWrite)]
    public async Task the_component_shows_the_text_the_step_set_on_its_outer_object()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropObjectMemberWrite);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Rename outer" }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: Gamma, renamed");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(BenchPages.PropObjectMemberWrite);
        await Expect(untouched.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: Gamma");
    }
}
