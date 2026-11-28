using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASchedulesFieldNamesAPathPastOneMember(string host) : FusionFixture(host)
{
    private const string AdasShift = "Ada Brooks Begin From Monday, November 9, 2026 at 9:00:00 AM GMT-08:00 Ends At Monday, November 9, 2026 at 5:00:00 PM GMT-08:00";

    [Test]
    [FailsFor(Promises.M21_F39.M21_X1057_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_path()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.LedShiftsByLead);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionScheduleFields names 's => s.Lead.Name', a path past one member of the event, "
                + "and the schedule reads each field as one member of an event; name one member, such as item => item.Subject.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.LedShiftsByCarer);
        await Expect(rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = AdasShift, Exact = true })).ToBeVisibleAsync();
    }
}
