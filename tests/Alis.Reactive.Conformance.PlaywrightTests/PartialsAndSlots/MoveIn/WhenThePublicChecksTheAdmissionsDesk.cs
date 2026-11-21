using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.PublicInformation))]
[Category(Hosts.PublicInformationName)]
public sealed class WhenThePublicChecksTheAdmissionsDesk(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X262_0)]
    public async Task public_information_reads_the_desk_status_and_keeps_the_draft_private()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionInformation);
        var sent = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check desk availability", Exact = true }).ClickAsync(), "**/desk-status");
        Assert.That(sent.Headers.ContainsKey("requestverificationtoken"), Is.False);
        await Expect(world.Page.Locator("#desk-status")).ToHaveTextAsync("Admission desk available");
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.GetByText("This page is unavailable on the public admissions desk.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await world.Page.GotoAsync(CarePages.AdmissionInformation);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check desk availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#desk-status")).ToHaveTextAsync("Admission desk available");
    }
}
