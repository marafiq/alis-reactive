using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonBodySendsAnEnumPastAListPosition(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_30_0)]
    public async Task the_item_carries_the_form_its_endpoint_binds_and_a_form_no_value_is_held_as_stops_the_request()
    {
        var missed = await NewWorld();
        await missed.Page.GotoAsync(CarePages.DoseLinesOfAMissedDose);
        var sent = await missed.Page.RunAndWaitForRequestAsync(() => missed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the doses", Exact = true }).ClickAsync(), "**/dose-lines/bound");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.PostData, Is.EqualTo("{\"doses\":[{\"status\":\"missed\"}]}"));
        await Expect(missed.Page.Locator("#bound")).ToHaveTextAsync("Bound Missed unnoted");

        var skipped = await NewWorld();
        await skipped.Page.GotoAsync(CarePages.DoseLinesOfASkippedDose);
        await skipped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the doses", Exact = true }).ClickAsync();
        await Expect(skipped.Page.Locator("#sending")).ToHaveTextAsync("Finished");
        await Expect(skipped.Page.Locator("#bound")).ToHaveTextAsync("-");
        Assert.That(skipped.ConsoleErrors, Has.Count.EqualTo(1));
        Assert.That(skipped.ConsoleErrors.Single(), Is.EqualTo("The request's member 'Doses[0].Status' holds \"Skipped\"; its enum forms are \"Due\", \"Given\", \"Missed\"."));
    }
}
