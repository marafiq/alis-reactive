using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPivotsFieldNamesAPathPastOneMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F34.M21_X1056_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.LedUnitsByLead);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionPivotViewFields names 'u => u.Lead.Name', a path past one member of the row, "
                + "and the pivot reads each field as one member of a row; name one member, such as row => row.Ward.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.LedUnitsByWard);
        await Expect(rendered.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "North 1", Exact = true })).ToBeVisibleAsync();
    }
}
