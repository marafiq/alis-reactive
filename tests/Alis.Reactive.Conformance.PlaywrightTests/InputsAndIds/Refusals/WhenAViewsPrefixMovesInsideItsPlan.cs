using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAViewsPrefixMovesInsideItsPlan(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_16_0)]
    public async Task the_page_is_refused_where_its_plan_renders_naming_the_view_the_prefix_and_the_partial_door()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.FamilyPrefixMovedInsideItsPlan);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: The field prefix of /Refusals/InputsAndIds/FamilyPrefixMovedInsideItsPlan.cshtml changed from none to 'NextOfKin' inside its plan, "
                + "so what the view renders after the change posts under names its plan does not send or check: "
                + "render a member's view with Html.PartialFor, which leaves the view's prefix as it is.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.FamilyPrefixLeftAsItsPlanStarted);
        await rendered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Guardian", Exact = true }).FillAsync("Ada");
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Guardian: Ada");
    }
}
