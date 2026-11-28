using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScheduleResourcesFieldNamesAPathPastOneMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F39.M21_X1059_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.LedTeamsByLead);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionScheduleFields names 't => t.Lead.Name', a path past one member of the resource, "
                + "and the schedule reads each field as one member of a resource; name one member, such as item => item.Subject.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.LedTeamsByName);
        await Expect(rendered.Page.GetByRole(AriaRole.Cell, new PageGetByRoleOptions { Name = "Day team", Exact = true })).ToBeVisibleAsync();
    }
}
