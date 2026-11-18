using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DateConverter))]
[Category(Hosts.DateConverterName)]
public sealed class WhenALiveMemberIsSetFromRecordsWhoseDateTheAppsConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_10_1)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_and_the_member()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.BedRegister);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A live object takes 'register.Beds' as JavaScript holds its type, and the app's JSON converter writes 'Record.Day' otherwise; "
                + "give the member a JSON converter of its own, or pass the value from the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var withoutTheAppsConverter = await NewWorldUnder(Hosts.Net10Name);
        await withoutTheAppsConverter.Page.GotoAsync(RefusalPages.BedRegister);
        await Expect(withoutTheAppsConverter.Page.Locator("#register").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Bed 1", "Bed 2"]);
    }
}
