using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADateInputIsSentInAJsonBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_42_0)]
    public async Task the_date_is_sent_as_its_wire_text_and_left_out_while_blank()
    {
        var due = await NewWorld();
        await due.Page.GotoAsync(CarePages.FollowUp);
        await due.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason", Exact = true }).FillAsync("Falls");
        var sent = await due.Page.RunAndWaitForRequestAsync(() => due.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send to the GP", Exact = true }).ClickAsync(), "**/follow-up/referrals");
        Assert.That(sent.PostData, Is.EqualTo("{\"due\":\"2026-12-01\",\"reason\":\"Falls\"}"));
        await Expect(due.Page.Locator("#followed-up")).ToHaveTextAsync("Falls: due 2026-12-01 00:00");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.FollowUp);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason", Exact = true }).FillAsync("Falls");
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).FillAsync("");
        var blank = await cleared.Page.RunAndWaitForRequestAsync(() => cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send to the GP", Exact = true }).ClickAsync(), "**/follow-up/referrals");
        Assert.That(blank.PostData, Is.EqualTo("{\"reason\":\"Falls\"}"));
        await Expect(cleared.Page.Locator("#followed-up")).ToHaveTextAsync("Falls: due 0001-01-01 00:00");
    }
}
