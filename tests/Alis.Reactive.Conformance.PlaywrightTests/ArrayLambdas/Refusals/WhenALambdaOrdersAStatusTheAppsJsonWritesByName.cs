using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaOrdersAStatusTheAppsJsonWritesByName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_12_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_ordering()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DosesNoLongerDue);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Status > DoseStatus.Due' orders a value the app's JSON writes as text, which does not order as the value does in .NET; "
                + "compare it with == or !=, or order on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var enumsAsNumbers = await NewWorldUnder(Hosts.EnumsAsNumbersName);
        await enumsAsNumbers.Page.GotoAsync(RefusalPages.DosesNoLongerDue);
        await Expect(enumsAsNumbers.Page.Locator("#count")).ToHaveTextAsync("2");
    }
}
