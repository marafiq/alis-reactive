using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesADateItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_43_1)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_comparison()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.VisitsAtAConvertedStart);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'v.At == start' compares a date or a time its own converter writes, whose text the converter decides; "
                + "compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.VisitsAtAStart);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
