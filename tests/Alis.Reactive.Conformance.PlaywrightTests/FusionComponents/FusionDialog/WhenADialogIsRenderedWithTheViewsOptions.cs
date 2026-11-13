using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADialogIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F18.M21_X1011_0)]
    public async Task the_dialog_starts_as_the_view_sets_it_and_a_step_shows_it_at_its_id()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.DischargeChecklist);
        await Expect(closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open checklist", Exact = true })).ToBeVisibleAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Discharge checklist", Exact = true })).ToBeHiddenAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open checklist", Exact = true }).ClickAsync();

        await Expect(closed.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Discharge checklist", Exact = true })).ToBeVisibleAsync();

        var shown = await NewWorld();
        await shown.Page.GotoAsync(CarePages.DischargeChecklistShown);

        await Expect(shown.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Discharge checklist", Exact = true })).ToBeVisibleAsync();
    }
}
