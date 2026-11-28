using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NoAntiforgeryChoice))]
[Category(Hosts.NoAntiforgeryChoiceName)]
public sealed class WhenAnUnsafeRequestRendersInAnAppThatChoseNoAntiforgery(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_26_0)]
    public async Task the_page_is_refused_where_it_renders_naming_the_three_choices()
    {
        var noChoice = await NewWorld();
        await noChoice.Page.GotoAsync(RefusalPages.IntakeFiling);
        await Expect(noChoice.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'POST /Intake' sends no antiforgery token, since the app chose none: "
                + "call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() in AddReactiveRequests.",
            Exact = true,
        })).ToBeVisibleAsync();

        var tokenInTheHeader = await NewWorldUnder(Hosts.Net10Name);
        await tokenInTheHeader.Page.GotoAsync(RefusalPages.IntakeFiling);
        var sent = await tokenInTheHeader.Page.RunAndWaitForRequestAsync(() => tokenInTheHeader.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync(), "**/Intake");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(tokenInTheHeader.Page.Locator("#intake-filed")).ToHaveTextAsync("Filed the intake of Ada Hart");
    }
}
