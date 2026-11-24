using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADateInputIsLeftBlank(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_41_0)]
    public async Task the_date_is_left_out_of_form_data_so_the_endpoint_binds_its_start()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.FollowUp);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason", Exact = true }).FillAsync("Falls");
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Due", Exact = true }).FillAsync("");
        var sent = await cleared.Page.RunAndWaitForRequestAsync(() => cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save follow-up", Exact = true }).ClickAsync(), "**/follow-up/follow-ups");
        Assert.That(sent.PostData, Is.EqualTo("Reason=Falls"));
        await Expect(cleared.Page.Locator("#followed-up")).ToHaveTextAsync("Falls: due 0001-01-01 00:00");

        var due = await NewWorld();
        await due.Page.GotoAsync(CarePages.FollowUp);
        await due.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason", Exact = true }).FillAsync("Falls");
        var dated = await due.Page.RunAndWaitForRequestAsync(() => due.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save follow-up", Exact = true }).ClickAsync(), "**/follow-up/follow-ups");
        Assert.That(dated.PostData, Is.EqualTo("Due=2026-12-01T00%3A00%3A00&Reason=Falls"));
        await Expect(due.Page.Locator("#followed-up")).ToHaveTextAsync("Falls: due 2026-12-01 00:00");
    }
}
