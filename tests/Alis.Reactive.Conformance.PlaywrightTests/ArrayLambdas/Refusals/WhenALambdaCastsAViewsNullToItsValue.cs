using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaCastsAViewsNullToItsValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_48_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_value_it_cast()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DosesOfNoPriority);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'none' is null when the view renders, and the cast takes its value, which .NET refuses; "
                + "compare the nullable itself, such as d => d.Priority == limit.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DosesOfAPriority);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
