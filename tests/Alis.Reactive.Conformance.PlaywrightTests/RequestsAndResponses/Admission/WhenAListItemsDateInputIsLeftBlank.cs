using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListItemsDateInputIsLeftBlank(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_44_0)]
    public async Task it_stops_the_request_where_it_is_its_items_only_field_and_binds_its_start_beside_another()
    {
        var followUpCleared = await NewWorld();
        await followUpCleared.Page.GotoAsync(CarePages.VisitRound);
        await followUpCleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await followUpCleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Follow-up visit", Exact = true }).FillAsync("");
        await followUpCleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save round", Exact = true }).ClickAsync();
        await Expect(followUpCleared.Page.Locator("#save-status")).ToHaveTextAsync("Saving the round");
        await Expect(followUpCleared.Page.Locator("#saved-round")).ToBeEmptyAsync();
        Assert.That(followUpCleared.ConsoleErrors, Has.Count.EqualTo(1));

        var firstCleared = await NewWorld();
        await firstCleared.Page.GotoAsync(CarePages.VisitRound);
        await firstCleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await firstCleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "First visit", Exact = true }).FillAsync("");
        var sent = await firstCleared.Page.RunAndWaitForRequestAsync(() => firstCleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save round", Exact = true }).ClickAsync(), "**/visit-round/rounds");
        Assert.That(sent.PostData, Is.EqualTo("Visits%5B0%5D.Note=Bloods&Visits%5B1%5D.When=2026-12-01T00%3A00%3A00&Resident=Ada"));
        await Expect(firstCleared.Page.Locator("#saved-round")).ToHaveTextAsync("Ada: 0001-01-01, 2026-12-01");
    }
}
