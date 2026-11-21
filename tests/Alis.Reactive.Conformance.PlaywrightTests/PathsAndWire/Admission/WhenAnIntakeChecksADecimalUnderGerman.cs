using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnIntakeChecksADecimalUnderGerman(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_6_0)]
    public async Task one_and_a_half_passes_and_half_shows_the_strength_message()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.IntakeAssessmentInGerman);
        await intake.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("1,5");
        await intake.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(intake.Page.Locator("#intake-status")).ToHaveTextAsync("Saved");
        await Expect(intake.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Strength", Exact = true })
            .GetByText("Give a strength above 1.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(intake.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var tooWeak = await NewWorld();
        await tooWeak.Page.GotoAsync(CarePages.IntakeAssessmentInGerman);
        await tooWeak.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("0,5");
        await tooWeak.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(tooWeak.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Strength", Exact = true })
            .GetByText("Give a strength above 1.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(tooWeak.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Give a strength above 1."]);
        await Expect(tooWeak.Page.Locator("#intake-status")).ToHaveTextAsync("Not saved");
    }
}
