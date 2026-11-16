using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHeaderIsReadAsTextPastUsAscii(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_13_0)]
    public async Task no_request_is_sent_and_the_console_names_the_header()
    {
        var pastUsAscii = await NewWorld();
        await pastUsAscii.Page.GotoAsync(CarePages.AdmissionApplication);
        await pastUsAscii.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Zoë");
        await pastUsAscii.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply", Exact = true }).ClickAsync();
        await Expect(pastUsAscii.Page.Locator("#application-closed")).ToHaveTextAsync("Closed");
        await Expect(pastUsAscii.Page.Locator("#application-status")).ToHaveTextAsync("Sending");
        Assert.That(pastUsAscii.ConsoleErrors, Has.Count.EqualTo(1));
        Assert.That(pastUsAscii.ConsoleErrors.Single(), Does.Contain("X-Applicant"));

        var usAscii = await NewWorld();
        await usAscii.Page.GotoAsync(CarePages.AdmissionApplication);
        await usAscii.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Zoe");
        await usAscii.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply", Exact = true }).ClickAsync();
        await Expect(usAscii.Page.Locator("#application-status")).ToHaveTextAsync("X-Applicant Zoe");
    }
}
