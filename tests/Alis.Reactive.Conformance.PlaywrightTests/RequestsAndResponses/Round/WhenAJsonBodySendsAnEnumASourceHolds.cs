using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonBodySendsAnEnumASourceHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_29_0)]
    public async Task the_body_carries_the_form_its_endpoint_binds_and_a_form_no_value_is_held_as_stops_the_request()
    {
        var missed = await NewWorld();
        await missed.Page.GotoAsync(CarePages.ChartStatusOfAMissedDose);
        var sent = await missed.Page.RunAndWaitForRequestAsync(() => missed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the status", Exact = true }).ClickAsync(), "**/chart-status/bound");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.PostData, Is.EqualTo("{\"status\":\"missed\"}"));
        await Expect(missed.Page.Locator("#bound")).ToHaveTextAsync("Bound Missed");

        var skipped = await NewWorld();
        await skipped.Page.GotoAsync(CarePages.ChartStatusOfASkippedDose);
        await skipped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the status", Exact = true }).ClickAsync();
        await Expect(skipped.Page.Locator("#sending")).ToHaveTextAsync("Finished");
        await Expect(skipped.Page.Locator("#bound")).ToHaveTextAsync("-");
        Assert.That(skipped.ConsoleErrors, Has.Count.EqualTo(1));
        Assert.That(skipped.ConsoleErrors.Single(), Is.EqualTo("The request's member 'Status' holds \"Skipped\"; its enum forms are \"Due\", \"Given\", \"Missed\"."));
    }
}
