using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiveMemberIsSetFromAFieldItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_10_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_field_and_its_own_converter()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.StatementOfABalanceWrittenAsText);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A live object takes 'decimal' as JavaScript holds it, and 'owed.Due' is written by its own JSON converter; "
                + "pass the value from the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.StatementOfABalance);
        await Expect(rendered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount" })).ToHaveTextAsync("120");
    }
}
