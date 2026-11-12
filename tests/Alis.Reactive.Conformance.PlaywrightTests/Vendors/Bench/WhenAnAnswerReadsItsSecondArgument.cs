using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAnswerReadsItsSecondArgument(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_23_0)]
    public async Task the_answer_reads_the_record_its_component_passes_at_position_one()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsPairedAnswers);
        await bench.Page.Locator("#conformance-records").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-records").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ada", "Bea", "Cy" });
        await bench.Page.Locator("#conformance-keys").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-keys").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "count", "count", "count" });
    }
}
