using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATemplateConditionAddsTwoMembers(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X4_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_both_members()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ContactBoardByAddedContacts);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A template condition adds Visits and Calls, which Syncfusion's template engine reads as one word; "
                + "compare with a member the server computes on the item.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.ContactBoardByMultipliedContacts);
        await Expect(rendered.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ada", "Busy", "Ruth", "Quiet" });
    }
}
