using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATemplateConditionOrdersAnEnumWrittenByName(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X10_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_ordering()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.BedTurnoverOrdered);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'm.Stage > TurnoverStage.Stripped' orders a value the app's JSON writes as text, "
                + "which does not order as the value does in .NET; compare it with == or !=, or order on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.BedTurnoverCompared);
        await Expect(rendered.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Bed 1", "ready", "Bed 2", "stripped" });
    }
}
