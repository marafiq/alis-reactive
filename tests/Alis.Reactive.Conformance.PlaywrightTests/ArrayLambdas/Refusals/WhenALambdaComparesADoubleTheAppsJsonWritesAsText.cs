using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NumbersAsText))]
[Category(Hosts.NumbersAsTextName)]
public sealed class WhenALambdaComparesADoubleTheAppsJsonWritesAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_43_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_comparison()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.FridgeReadingsAtZero);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r.Temperature == 0.0' compares a float or double the JSON writes as text, whose text does not equal as the number does, "
                + "as -0 and 0 or NaN and NaN; compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var numbersAsJsonNumbers = await NewWorldUnder(Hosts.Net10Name);
        await numbersAsJsonNumbers.Page.GotoAsync(RefusalPages.FridgeReadingsAtZero);
        await Expect(numbersAsJsonNumbers.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
