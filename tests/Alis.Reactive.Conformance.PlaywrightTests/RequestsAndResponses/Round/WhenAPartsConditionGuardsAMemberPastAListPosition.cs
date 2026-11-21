using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartsConditionGuardsAMemberPastAListPosition(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_31_0)]
    public async Task the_member_is_sent_in_its_item_when_the_condition_holds_and_leaves_it_in_its_place_when_it_fails()
    {
        var late = await NewWorld();
        await late.Page.GotoAsync(CarePages.DoseLinesOfALateMissedDose);
        var lateSent = await late.Page.RunAndWaitForRequestAsync(() => late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the doses", Exact = true }).ClickAsync(), "**/dose-lines/bound");
        Assert.That(lateSent.PostData, Is.EqualTo("{\"doses\":[{\"status\":\"missed\",\"note\":\"Late\"}]}"));
        await Expect(late.Page.Locator("#bound")).ToHaveTextAsync("Bound Missed Late");

        var onTime = await NewWorld();
        await onTime.Page.GotoAsync(CarePages.DoseLinesOfAMissedDose);
        var onTimeSent = await onTime.Page.RunAndWaitForRequestAsync(() => onTime.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the doses", Exact = true }).ClickAsync(), "**/dose-lines/bound");
        Assert.That(onTimeSent.PostData, Is.EqualTo("{\"doses\":[{\"status\":\"missed\"}]}"));
        await Expect(onTime.Page.Locator("#bound")).ToHaveTextAsync("Bound Missed unnoted");
    }
}
