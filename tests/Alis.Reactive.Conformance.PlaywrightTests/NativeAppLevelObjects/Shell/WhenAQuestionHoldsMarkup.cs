using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAQuestionHoldsMarkup(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.M25_X6_0)]
    public async Task the_dialog_shows_the_question_as_typed()
    {
        var marked = await NewWorld();
        await marked.Page.GotoAsync(CarePages.DischargeNote);
        await marked.Page.GetByLabel("Note", new PageGetByLabelOptions { Exact = true }).FillAsync("<b>Discharge Ada?</b>");
        await marked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = marked.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("<b>Discharge Ada?</b>", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.DischargeNote);
        await plain.Page.GetByLabel("Note", new PageGetByLabelOptions { Exact = true }).FillAsync("Discharge Bo?");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = plain.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asked.GetByText("Discharge Bo?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
