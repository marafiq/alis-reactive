using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASelectTakesAMemberItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_45_0)]
    public async Task the_selected_items_are_compared_in_the_members_own_form_and_a_sum_of_them_is_refused()
    {
        var counted = await NewWorld();
        await counted.Page.GotoAsync(CarePages.GivenChartsCounted);
        await Expect(counted.Page.Locator("#given-chart-count")).ToHaveTextAsync("3");

        var summed = await NewWorld();
        await summed.Page.GotoAsync(CarePages.VialsSummedThroughASelect);
        await Expect(summed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Supply.Vials' is written by its class's JSON number handling, unlike the app's options, "
                + "so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.",
            Exact = true,
        })).ToBeVisibleAsync();
    }
}
