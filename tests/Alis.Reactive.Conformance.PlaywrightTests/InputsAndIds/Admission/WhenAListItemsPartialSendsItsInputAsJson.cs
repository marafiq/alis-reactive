using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListItemsPartialSendsItsInputAsJson(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_10_0)]
    public async Task a_cards_name_is_bound_at_the_position_of_the_list_its_card_was_rendered_for()
    {
        var circle = await NewWorld();
        await circle.Page.GotoAsync(CarePages.Circle);
        await circle.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Second member", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await circle.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(circle.Page.Locator("#bound")).ToHaveTextAsync("member 1 Ada and member 0 none");

        var firstNamed = await NewWorld();
        await firstNamed.Page.GotoAsync(CarePages.Circle);
        await firstNamed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "First member", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await firstNamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(firstNamed.Page.Locator("#bound")).ToHaveTextAsync("member 0 Ada and member 1 none");
    }
}
