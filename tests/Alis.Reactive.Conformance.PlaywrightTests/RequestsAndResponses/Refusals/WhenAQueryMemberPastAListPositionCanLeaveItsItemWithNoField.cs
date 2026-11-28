using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAQueryMemberPastAListPositionCanLeaveItsItemWithNoField(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_46_0)]
    public async Task the_page_is_refused_where_the_view_renders_and_renders_beside_a_member_of_the_item_always_sent()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.FollowUpBookingByItsDate);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: The request sends 'Visits[1].When' in its query past a list position from a value it leaves out when it reads nothing, "
                + "and no member of 'Visits[1]' is always sent, so the item would send no field and the endpoint's list would end before it on both hosts; "
                + "send beside it a member of the item that is always sent, a literal, a text or a nullable member outside every condition, or send the list in a JSON body.",
            Exact = true,
        })).ToBeVisibleAsync();

        var noted = await NewWorld();
        await noted.Page.GotoAsync(RefusalPages.FollowUpBookingWithItsNote);
        var sent = await noted.Page.RunAndWaitForRequestAsync(() => noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book follow-up", Exact = true }).ClickAsync(), "**/follow-up-booking/bookings**");
        Assert.That(sent.Url, Does.EndWith("/follow-up-booking/bookings?Visits%5B0%5D.Note=Bloods&Visits%5B1%5D.When=2026-12-01&Visits%5B1%5D.Note=Review"));
        await Expect(noted.Page.Locator("#booked")).ToHaveTextAsync("2 visits, the follow-up on 2026-12-01 for Review");
    }
}
