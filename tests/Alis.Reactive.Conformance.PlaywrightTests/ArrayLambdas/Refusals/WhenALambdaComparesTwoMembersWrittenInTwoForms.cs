using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesTwoMembersWrittenInTwoForms(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_42_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_its_own_converter_writes()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DosesChartedAsPlanned);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Charted == d.Planned' compares two members, and its own JSON converter writes 'd.Charted' "
                + "unlike the app's options, so the browser does not compare them as .NET does; compare them on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DosesGivenAsPlanned);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
