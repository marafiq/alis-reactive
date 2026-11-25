using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListItemsLinkInputIsLeftEmpty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_45_0)]
    public async Task it_stops_the_request_where_it_is_its_items_only_field_and_is_left_out_beside_another()
    {
        var followUpEmpty = await NewWorld();
        await followUpEmpty.Page.GotoAsync(CarePages.VideoCalls);
        await followUpEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await followUpEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "First call", Exact = true }).FillAsync("https://meet.example.org/ada");
        await followUpEmpty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save calls", Exact = true }).ClickAsync();
        await Expect(followUpEmpty.Page.Locator("#save-status")).ToHaveTextAsync("Saving the calls");
        await Expect(followUpEmpty.Page.Locator("#saved-calls")).ToBeEmptyAsync();
        Assert.That(followUpEmpty.ConsoleErrors, Has.Count.EqualTo(1));

        var firstEmpty = await NewWorld();
        await firstEmpty.Page.GotoAsync(CarePages.VideoCalls);
        await firstEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await firstEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Follow-up call", Exact = true }).FillAsync("https://meet.example.org/review");
        var sent = await firstEmpty.Page.RunAndWaitForRequestAsync(() => firstEmpty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save calls", Exact = true }).ClickAsync(), "**/video-calls/calls");
        Assert.That(sent.PostData, Is.EqualTo("Calls%5B0%5D.Note=Bloods&Calls%5B1%5D.Link=https%3A%2F%2Fmeet.example.org%2Freview&Resident=Ada"));
        await Expect(firstEmpty.Page.Locator("#saved-calls")).ToHaveTextAsync("Ada: no link, https://meet.example.org/review");
    }
}
