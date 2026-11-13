using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NumbersAsText))]
[Category(Hosts.NumbersAsTextName)]
public sealed class WhenAConditionComparesADoubleTheAppsJsonWritesAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_22_0)]
    public async Task the_page_is_refused_where_the_view_renders_and_told_to_compare_the_double_on_the_server()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.FluidRatioAtZero);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A condition compares a float or double the JSON writes as text, whose text does not equal as the number does, "
                + "as -0 and 0 or NaN and NaN; compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var numbersAsJsonNumbers = await NewWorldUnder(Hosts.Net10Name);
        await numbersAsJsonNumbers.Page.GotoAsync(RefusalPages.FluidRatioAtZero);
        await numbersAsJsonNumbers.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the ratio", Exact = true }).ClickAsync();
        await Expect(numbersAsJsonNumbers.Page.Locator("#ratio")).ToHaveTextAsync("balanced");
    }
}
