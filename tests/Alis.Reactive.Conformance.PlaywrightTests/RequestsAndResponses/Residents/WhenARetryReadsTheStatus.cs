using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARetryReadsTheStatus(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_15_0)]
    public async Task a_500_is_answered_once_and_a_503_is_sent_again()
    {
        var counted = await NewWorld();
        await counted.Seed(Seed.TheEndpointAnswers(CareEndpoints.FailingAttempt, 500));
        await counted.Seed(Seed.TheEndpointAnswers(CareEndpoints.RetriedAttempt, 503, 200));
        await counted.InstallTheClock();
        await counted.Page.GotoAsync(CarePages.AttemptsCounted);
        await counted.HoldTheClock();
        await counted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load first", Exact = true }).ClickAsync();
        await Expect(counted.Page.Locator("#attempts-log")).ToHaveTextAsync("error");
        Assert.That(counted.SentTo(CareEndpoints.FailingAttempt), Is.EqualTo(1));
        await counted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load second", Exact = true }).ClickAsync();
        await counted.RunTheClockUntilItSends(CareEndpoints.RetriedAttempt, 2);
        await Expect(counted.Page.Locator("#second-status")).ToHaveTextAsync("Answered");

        var once = await NewWorld();
        await once.Seed(Seed.TheEndpointAnswers(CareEndpoints.RetriedAttempt, 503, 200));
        await once.Page.GotoAsync(CarePages.AttemptsCountedOnce);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load second", Exact = true }).ClickAsync();
        await Expect(once.Page.Locator("#second-status")).ToHaveTextAsync("Unavailable");
    }
}
