using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMethodsArgumentFallsBackFromANull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_6_1)]
    public async Task the_method_receives_the_fallback_for_a_null_and_the_value_that_is_held()
    {
        var silent = await NewWorld();
        await silent.Page.GotoAsync(CarePages.PumpLogWithNoAlarm);
        await silent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log value", Exact = true }).ClickAsync();
        await Expect(silent.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "text No value" });

        var alarmed = await NewWorld();
        await alarmed.Page.GotoAsync(CarePages.PumpLogWithAnAlarm);
        await alarmed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log value", Exact = true }).ClickAsync();
        await Expect(alarmed.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "text Air in line" });
    }
}
