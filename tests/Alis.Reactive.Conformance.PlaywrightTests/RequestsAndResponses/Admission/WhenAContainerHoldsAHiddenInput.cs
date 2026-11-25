using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAContainerHoldsAHiddenInput(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_47_0)]
    public async Task its_form_body_sends_the_hidden_input_under_its_name_and_its_query_sends_none()
    {
        var handover = await NewWorld();
        await handover.Page.GotoAsync(CarePages.IntakeHandover);
        await handover.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await handover.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(handover.Page.Locator("#received")).ToHaveTextAsync("Desk Front; Name Ada; Shift Night; Ward North");

        await handover.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Look up", Exact = true }).ClickAsync();
        await Expect(handover.Page.Locator("#received")).ToHaveTextAsync("Name Ada");
    }
}
