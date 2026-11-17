using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABulletChartsFieldNamesAPathPastOneMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F7.M21_X1041_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.WardFluidsByLead);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionBulletChartHtml.Fields names 'w => w.Lead.Name', a path past one member of the item, "
                + "and the chart reads each field as one member of an item; name one member, such as item => item.Taken.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.WardFluidsByWard);
        await Expect(rendered.Page.Locator("#ward-fluids").GetByText("North", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
