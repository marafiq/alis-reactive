using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenDataLacksAMemberNamedConstructor(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_8_0)]
    public async Task the_member_reads_its_start_and_never_what_every_object_inherits()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionBindingConstructors);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the bindings with no constructor", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#constructor-count")).ToHaveTextAsync("4");

        var constructed = await NewWorld();
        await constructed.Seed(Seed.TheComponentsBetaConstructor("own"));
        await constructed.Page.GotoAsync(BenchPages.ExecutionBindingConstructors);
        await constructed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the bindings with no constructor", Exact = true }).ClickAsync();
        await Expect(constructed.Page.Locator("#constructor-count")).ToHaveTextAsync("3");
    }
}
