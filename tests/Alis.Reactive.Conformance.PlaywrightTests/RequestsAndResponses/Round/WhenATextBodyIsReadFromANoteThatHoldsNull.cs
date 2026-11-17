using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextBodyIsReadFromANoteThatHoldsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_33_0)]
    public async Task the_empty_text_is_sent_and_a_note_that_holds_text_is_sent_as_it_is()
    {
        var unnoted = await NewWorld();
        await unnoted.Page.GotoAsync(CarePages.NightHandoverWithNoNote);
        await unnoted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(unnoted.Page.Locator("#handed-over")).ToHaveTextAsync("Handed over with no note");

        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.NightHandoverWithANote);
        var handed = await noted.Page.RunAndWaitForRequestAsync(() => noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync(), "**/night-handover/received");
        Assert.That(handed.Method, Is.EqualTo("POST"));
        Assert.That(handed.PostData, Is.EqualTo("Slept well"));
        await Expect(noted.Page.Locator("#handed-over")).ToHaveTextAsync("Handed over: Slept well");
    }
}
