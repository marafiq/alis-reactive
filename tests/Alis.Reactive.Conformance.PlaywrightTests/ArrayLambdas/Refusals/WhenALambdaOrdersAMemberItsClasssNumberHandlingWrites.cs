using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaOrdersAMemberItsClasssNumberHandlingWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_40_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_its_classs_number_handling()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.SuppliedDosesOrdered);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Supply.Vials > 2' orders 'd.Supply.Vials', which its class's JSON number handling writes "
                + "unlike the app's options, so the browser does not order it as .NET does; order it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.SuppliedDosesCompared);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("1");
    }
}
