using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSendsAContainerOfAnotherModel(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_17_0)]
    public async Task the_request_stops_before_it_is_sent_with_one_error_naming_the_container_and_both_models()
    {
        var visit = await NewWorld();
        await visit.Page.GotoAsync(CarePages.VisitAdmit);
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the section", Exact = true }).ClickAsync();
        await Expect(visit.Page.Locator("#section")).ToHaveTextAsync("Visit of Ada's daughter");
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(visit.Page.Locator("#admitting")).ToHaveTextAsync("yes");
        await Expect(visit.Page.Locator("#received")).ToHaveTextAsync("-");
        Assert.That(visit.ConsoleErrors, Is.EqualTo(new[] { "Container 'visit' is rendered for model 'Alis.Reactive.Conformance.App.Care.FamilyVisit', not 'Alis.Reactive.Conformance.App.Care.Admission'." }));

        var own = await NewWorld();
        await own.Page.GotoAsync(CarePages.VisitAdmitOwnSection);
        await own.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the section", Exact = true }).ClickAsync();
        await own.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("B4");
        await own.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(own.Page.Locator("#received")).ToHaveTextAsync("Room B4");
    }
}
