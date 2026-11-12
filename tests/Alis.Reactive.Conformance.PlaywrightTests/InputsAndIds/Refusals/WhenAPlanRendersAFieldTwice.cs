using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPlanRendersAFieldTwice(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F2.X14_5_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_input_and_the_id_it_renders_again()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.DepartureRoomTwice);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Input 'Room' renders element id 'Alis_Reactive_Conformance_App_Refusals_DeparturePage__Room' a second time in this plan: "
                + "render the field once, or each copy for its own member with Html.PartialFor.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.DepartureRoomOnce);
        await rendered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("B-12");
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#saved")).ToHaveTextAsync("Departing from room B-12");
    }
}
