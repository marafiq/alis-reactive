using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAViewsPrefixMovesAfterItsPlanStarted(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_4_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_view_the_prefix_and_the_input()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.MoveInPrefixAfterItsPlan);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: The field prefix of /Refusals/InputsAndIds/MoveInPrefixAfterItsPlan.cshtml changed from none to 'NextOfKin' after its plan started, "
                + "so input 'NextOfKin.Name' would post under a name its plan does not send or check.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.MoveInPrefixBeforeItsPlan);
        await rendered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Next of kin", Exact = true }).FillAsync("Ada");
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Next of kin: Ada");
    }
}
