namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

/// <summary>
/// A view that renders its plan itself (<c>plan.Render(services)</c>) resolves the rules its requests validate
/// by from the services it hands over: with the request's services the browser checks the rule and holds the
/// request; with services that hold no rules the page is refused when it renders, with the message the
/// developer reads.
/// </summary>
[TestFixture]
public sealed class WhenAPlanIsRenderedWithTheServicesGiven : AppPageTest
{
    private readonly List<string> _sends = new();

    [SetUp]
    public void WatchTheEndpoint()
    {
        _sends.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Plans/Serviced", StringComparison.Ordinal))
                lock (_sends) _sends.Add(request.Method);
        };
    }

    [Test]
    public async Task the_rules_of_the_services_given_hold_the_request_until_they_pass()
    {
        await Open("/Plans/Serviced");
        var send = Page.GetByRole(AriaRole.Button, new() { Name = "Send the visitor", Exact = true });

        await send.ClickAsync();
        await Expect(Page.GetByText("Name the visitor.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_sends) Assert.That(_sends, Is.Empty, "the browser held the request");

        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Ada");
        await send.ClickAsync();
        await Expect(Page.Locator("#serviced-bound")).ToHaveTextAsync("visitor Ada");
    }

    [Test]
    public async Task services_that_hold_no_rules_refuse_a_plan_whose_request_validates()
    {
        await Page.GotoAsync(BaseUrl + "/Plans/Serviced?withoutRules=true");

        await Expect(Page.GetByText(
            "Request at '/Plans/Serviced' specifies validation source 'ServicedRules', but no IBrowserRuleSource is registered in DI.").First)
            .ToBeVisibleAsync();
    }
}
