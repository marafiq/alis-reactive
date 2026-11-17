using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASplitButtonsTypedItemsAreListed(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F44.M21_X1035_0)]
    public async Task the_list_shows_each_items_text_and_the_chosen_one_reaches_the_views_handler()
    {
        var cancelling = await NewWorld();
        await cancelling.Page.GotoAsync(CarePages.VisitChoices);
        await cancelling.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "dropdownbutton", Exact = true }).ClickAsync();

        await Expect(cancelling.Page.GetByRole(AriaRole.Menuitem)).ToHaveTextAsync(new[] { "Reschedule a visit", "Cancel a visit" });
        await cancelling.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Cancel a visit", Exact = true }).ClickAsync();
        await Expect(cancelling.Page.Locator("#visit-choice")).ToHaveTextAsync("Cancel a visit");

        var rescheduling = await NewWorld();
        await rescheduling.Page.GotoAsync(CarePages.VisitChoices);
        await rescheduling.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "dropdownbutton", Exact = true }).ClickAsync();
        await rescheduling.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Reschedule a visit", Exact = true }).ClickAsync();

        await Expect(rescheduling.Page.Locator("#visit-choice")).ToHaveTextAsync("Reschedule a visit");
    }
}
