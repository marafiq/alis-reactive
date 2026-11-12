using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaLooksUpAFractionTheJsonWritesAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_54_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_lookup()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.CopaysLookedUp);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'copays.Contains(d.Copay)' looks for a fraction the JSON writes as text, and the browser's lookup "
                + "compares its text, which does not equal as the number does, as 0.10 and 0.1 or -0 and 0; compare a decimal with == for each value, "
                + "such as d => d.Copay == 0.10m || d.Copay == 0.25m, or look it up on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.CopaysCompared);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
