using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DateConverter))]
[Category(Hosts.DateConverterName)]
public sealed class WhenEditedRowsHoldADateTheAppsConverterWrites(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X209_0)]
    public async Task the_batch_page_is_refused_where_it_renders_naming_the_date()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ChargeBatch);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Changed' holds 'dueOn', which a JSON converter of the app's writes, and a JSON body sends it in the form its endpoint binds, "
                + "which the browser cannot read from that writing; set it from a literal, or set its other members.",
            Exact = true,
        })).ToBeVisibleAsync();

        var withoutTheAppsConverter = await NewWorldUnder(Hosts.Net10Name);
        await withoutTheAppsConverter.Page.GotoAsync(RefusalPages.ChargeBatch);
        await withoutTheAppsConverter.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the batch", Exact = true }).ClickAsync();
        await Expect(withoutTheAppsConverter.Page.Locator("#saved")).ToHaveTextAsync("No charges changed");
    }
}
