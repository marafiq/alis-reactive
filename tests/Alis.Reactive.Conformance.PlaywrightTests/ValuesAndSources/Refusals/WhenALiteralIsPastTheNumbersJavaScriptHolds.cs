using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiteralIsPastTheNumbersJavaScriptHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_13_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_the_value()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.LedgerPastWholeNumbers);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'ConformanceComponent.Amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, "
                + "which JavaScript reads as another value; write a finite number of at most 15 significant digits.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.LedgerWithinWholeNumbers);
        await Expect(rendered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount" })).ToHaveTextAsync("9007199254740991");
    }
}
