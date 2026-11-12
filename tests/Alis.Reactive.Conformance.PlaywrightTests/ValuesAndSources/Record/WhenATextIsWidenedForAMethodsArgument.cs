using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextIsWidenedForAMethodsArgument(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_7_2)]
    public async Task the_method_receives_the_text_the_source_holds_and_null_where_it_holds_null()
    {
        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.PumpCaptionOfANamedRound);
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take text", Exact = true }).ClickAsync();
        await Expect(named.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("Morning round");

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.PumpCaptionOfAnUnnamedRound);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take text", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("(null)");
    }
}
