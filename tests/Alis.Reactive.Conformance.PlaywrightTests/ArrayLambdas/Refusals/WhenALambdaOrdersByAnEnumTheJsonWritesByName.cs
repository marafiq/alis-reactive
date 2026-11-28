using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaOrdersByAnEnumTheJsonWritesByName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_28_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_key()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DosesSortedByTheirStatus);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Status' orders a value the app's JSON writes as text, which does not order as the value does in .NET; "
                + "order by a member the JSON writes as a number, or order on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DosesSortedByTheirMilligrams);
        await Expect(rendered.Page.Locator("#first-dose")).ToHaveTextAsync("Digoxin");
    }
}
