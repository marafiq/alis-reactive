using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAViewAnswersAComponentsCallback(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_14_0)]
    public async Task the_component_draws_for_each_item_the_text_the_views_answer_gives()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsAnswers);
        await bench.Page.Locator("#conformance-answered").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-answered").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ada", "Bea", "Cy" });
        await bench.Page.Locator("#conformance-plain").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-plain").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "1", "2", "3" });
    }
}
