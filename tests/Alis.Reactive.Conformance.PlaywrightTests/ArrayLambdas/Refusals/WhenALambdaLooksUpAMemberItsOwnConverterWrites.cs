using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaLooksUpAMemberItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_39_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_its_own_converter()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ChartedDosesLookedUp);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Charted' is written by its own JSON converter, unlike the app's options, "
                + "so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.ChartedDosesCompared);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("3");
    }
}
