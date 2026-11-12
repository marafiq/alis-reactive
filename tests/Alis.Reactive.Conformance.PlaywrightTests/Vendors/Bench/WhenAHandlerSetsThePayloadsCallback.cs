using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerSetsThePayloadsCallback(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_callback_payload_before)]
    public async Task the_component_asks_the_answer_the_handler_set_once_the_handler_returns()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15CallbackPayloadBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Answer", Exact = true })).ToHaveTextAsync("One");

        var silent = await NewWorld();
        await silent.Page.GotoAsync(BenchPages.EventMethod);
        await silent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(silent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(silent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Answer", Exact = true })).ToHaveTextAsync("One unanswered");
    }
}
