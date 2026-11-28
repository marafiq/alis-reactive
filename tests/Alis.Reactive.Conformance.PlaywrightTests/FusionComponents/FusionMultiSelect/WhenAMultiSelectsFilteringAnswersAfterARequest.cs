using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsFilteringAnswersAfterARequest(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.event_method)]
    public async Task the_list_shows_the_items_the_endpoint_answered()
    {
        var latex = await NewWorld();
        await latex.Page.GotoAsync(CarePages.AllergySearch);
        await latex.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await latex.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("lat");

        await Expect(latex.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Latex" });

        var unknown = await NewWorld();
        await unknown.Page.GotoAsync(CarePages.AllergySearch);
        await unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("xyz");

        await Expect(unknown.Page.GetByText("No records found", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
