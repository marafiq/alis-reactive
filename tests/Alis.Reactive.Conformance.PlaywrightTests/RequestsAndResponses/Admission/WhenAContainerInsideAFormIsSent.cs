using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAContainerInsideAFormIsSent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_16_0)]
    public async Task the_containers_request_sends_its_own_inputs_and_the_forms_request_sends_both()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.IntakeContact);
        await intake.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await intake.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555");
        await intake.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send contact", Exact = true }).ClickAsync();
        await Expect(intake.Page.Locator("#received")).ToHaveTextAsync("Phone 555");
        await intake.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send all", Exact = true }).ClickAsync();
        await Expect(intake.Page.Locator("#received")).ToHaveTextAsync("Name Ada, Phone 555");

        var unanswered = await NewWorld();
        await unanswered.Page.GotoAsync(CarePages.IntakeContact);
        await unanswered.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await unanswered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send contact", Exact = true }).ClickAsync();
        await Expect(unanswered.Page.Locator("#received")).ToHaveTextAsync("Phone");
    }
}
