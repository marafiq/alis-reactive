using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumInputHoldsATextNoValueNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X16_47_0)]
    public async Task the_check_fails_with_the_binders_message_beside_the_input_and_in_the_summary_and_nothing_is_sent()
    {
        var agency = await NewWorld();
        await agency.Page.GotoAsync(CarePages.NightCover);
        await agency.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await agency.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Agency cover");
        await agency.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync();
        await Expect(agency.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Cover", Exact = true })
            .GetByText("The value 'agency' is not valid for Cover.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(agency.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The value 'agency' is not valid for Cover." });
        await Expect(agency.Page.Locator("#saved-cover")).ToBeEmptyAsync();

        var twoCarers = await NewWorld();
        await twoCarers.Page.GotoAsync(CarePages.NightCover);
        await twoCarers.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await twoCarers.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Cover", Exact = true }).SelectOptionAsync("Two carers");
        await twoCarers.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save cover", Exact = true }).ClickAsync();
        await Expect(twoCarers.Page.Locator("#saved-cover")).ToHaveTextAsync("Ada: TwoCarers");
        await Expect(twoCarers.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
