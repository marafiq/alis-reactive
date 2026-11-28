using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DateConverter))]
[Category(Hosts.DateConverterName)]
public sealed class WhenAConditionOrdersADateTheAppsConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_57_1)]
    public async Task the_handover_page_is_refused_where_the_view_renders_naming_the_read()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.WardHandovers);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the handover", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'handover.At' compares a DateTime the app's JSON options write by a converter of their own, "
                + "whose text the browser cannot read as the value .NET holds; compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var withoutTheAppsConverter = await NewWorldUnder(Hosts.Net10Name);
        await withoutTheAppsConverter.Page.GotoAsync(RefusalPages.WardHandovers);
        await withoutTheAppsConverter.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the handover", Exact = true }).ClickAsync();
        await withoutTheAppsConverter.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check handover", Exact = true }).ClickAsync();
        await Expect(withoutTheAppsConverter.Page.Locator("#handover-status")).ToHaveTextAsync("early");
    }
}
