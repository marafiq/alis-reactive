using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEditorTemplateRendersAnInputThePagesRuleNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F2.X14_8_0)]
    public async Task the_pages_rule_checks_the_templates_input_and_its_message_shows_beside_it()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.KinDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByText("Name the next of kin.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true })).ToHaveIdAsync("Alis_Reactive_Conformance_App_Care_DeskKin__NextOfKin_Name");
        await Expect(desk.Page.Locator("#kin-status")).ToHaveTextAsync("Not saved");
        Assert.That(desk.SentTo(CareEndpoints.KinDeskDesks), Is.EqualTo(0));
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#kin-status")).ToHaveTextAsync("next of kin Ada");
        await Expect(desk.Page.GetByText("Name the next of kin.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var server = await NewWorld();
        await server.Page.GotoAsync(CarePages.KinDeskCheckedOnTheServer);
        var sent = await server.Page.RunAndWaitForRequestAsync(
            () => server.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync(),
            "**" + CareEndpoints.ServerCheckedKinDeskDesks);
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(server.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByText("Name the next of kin.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
