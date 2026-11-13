using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATemplateConditionComparesADecimal(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X1_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_the_constant()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.RiskBoardByADecimalScore);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A template condition compares FallScore with 2.5, which Syncfusion's template engine cannot read back as written; "
                + "compare with true, false, a whole number or plain text, or with a boolean member the server sets on the item.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.RiskBoardByAWholeScore);
        await Expect(rendered.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ada", "High", "Ruth", "Low" });
    }
}
