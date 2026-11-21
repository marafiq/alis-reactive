using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesADateItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_22_1)]
    public async Task the_page_is_refused_where_the_view_renders_and_told_to_compare_the_date_on_the_server()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DoseGivenAtAConvertedTime);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A condition compares a date or a time its own converter writes, whose text the converter decides; "
                + "compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DoseGivenAtATime);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the time", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#given")).ToHaveTextAsync("given with the round");
    }
}
