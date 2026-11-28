using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormsCheckFailsBeforeItsRequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_4_0)]
    public async Task nothing_is_sent_and_no_step_after_the_request_runs()
    {
        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.QuickAdmission);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(unnamed.SentTo(CareEndpoints.QuickAdmissionAdmissions), Is.EqualTo(0));
        await Expect(unnamed.Page.Locator("#admission-log")).ToHaveTextAsync("-");

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.QuickAdmission);
        await named.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Hart");
        var sent = await named.Page.RunAndWaitForRequestAsync(() => named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync(), "**" + CareEndpoints.QuickAdmissionAdmissions);
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(named.Page.Locator("#admission-log")).ToHaveTextAsync("Sent");
    }
}
