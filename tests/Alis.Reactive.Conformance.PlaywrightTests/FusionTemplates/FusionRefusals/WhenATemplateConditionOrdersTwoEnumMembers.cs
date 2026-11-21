using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATemplateConditionOrdersTwoEnumMembers(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X11_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_what_the_browser_orders()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.BedChecksByStage);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'm.Stage > m.Wanted' orders values the browser cannot order as .NET does; "
                + "order numbers, a char, or a date or a time, or order on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.BedChecksByCount);
        await Expect(rendered.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Bed 1", "ready", "Bed 2", "waiting" });
    }
}
