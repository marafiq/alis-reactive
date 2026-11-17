using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedRoundSavesADoseForAWard(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string Doses = Send + " > POST " + CareEndpoints.WardDoses;
    private const string Census = Send + " > GET " + CareEndpoints.WardCensus;

    [Test]
    [FailsFor(Promises.M26_F2.X20_8_0)]
    public async Task its_spans_hold_each_route_template_and_no_value_the_page_holds()
    {
        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.TheNorthWardsRound);
        await saved.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ruth");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(saved.Page.Locator("#census")).ToHaveTextAsync("12");
        await saved.Page.GotoAsync(WorldPages.Collector);
        await Expect(saved.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Doses + " completed 200",
            Doses + " > alis.validate validator WardRoundDoseRules rule set default failure count 0",
            Send + " completed",
            Census + " completed 200",
        ]);
        await saved.Page.GetByRole(AriaRole.Searchbox, new PageGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("{Ward}");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(saved.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Spans holding {Ward}: 2");
        await Expect(saved.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([Doses + " completed 200", Census + " completed 200"]);
        await saved.Page.GetByRole(AriaRole.Searchbox, new PageGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Ruth");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(saved.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Spans holding Ruth: 0");
        await saved.Page.GetByRole(AriaRole.Searchbox, new PageGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("North");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(saved.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Spans holding North: 0");

        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.TheNorthWardsRound);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Dose", Exact = true })
            .GetByText("Name is required.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await refused.Page.GotoAsync(WorldPages.Collector);
        await Expect(refused.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " stopped event alis.refused alis.reason=The body 'POST " + CareEndpoints.WardDoses + "' sends fails the rules WardRoundDose is registered with.",
        ]);
    }
}
